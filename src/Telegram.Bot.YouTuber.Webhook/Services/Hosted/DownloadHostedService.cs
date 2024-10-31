using Telegram.Bot.YouTuber.Webhook.Extensions;
using Telegram.Bot.YouTuber.Webhook.Services.Downloading;
using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Hosted;

public sealed class DownloadHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDownloadQueueService _downloadQueueService;
    private readonly ILogger<DownloadHostedService> _logger;

    public DownloadHostedService(IServiceProvider serviceProvider, IDownloadQueueService downloadQueueService, ILogger<DownloadHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _downloadQueueService = downloadQueueService;
        _logger = logger;
    }
    
    #region Overrides of BackgroundService

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-8.0&tabs=visual-studio

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DoWorkAsync(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Download service cancelled");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Download service failed");

                if (!stoppingToken.IsCancellationRequested)
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    #endregion

    private async Task DoWorkAsync(CancellationToken ct)
    {
        var context = await _downloadQueueService.DequeueAsync(ct);

        _logger.LogInformation("Started download task");
        
        using var scope = _serviceProvider.CreateScope();
        
        var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramService>();
        
        var downloadingClient = scope.ServiceProvider.GetRequiredService<IDownloadingClient>();
        
        var downloadingContext = await downloadingClient.DownloadAsync(context, ct);
        context.ApplyExternalContext(downloadingContext);
        
        if (context.IsSuccess)
        {
            if (downloadingContext.IsSkipped)
            {
                await telegramService.SendMessageAsync(context.ChatId, context.MessageId, "Nothing to download", ct);
            }
            else
            {
                var linkGenerator = scope.ServiceProvider.GetRequiredService<LinkGenerator>();
                var link = linkGenerator.GenerateFileLink(context);
                if (link is null)
                {
                    context.IsSuccess = false;
                    context.Error = new Exception("Failed to generate link");

                    await telegramService.SendInternalServerErrorAsync(context.ChatId, context.MessageId, context.Error, ct);
                }
                else
                {
                    await telegramService.SendMessageAsync(context.ChatId, context.MessageId, link, ct);
                }    
            }
        }
        else
        {
            await telegramService.SendMessageAsync(context.ChatId, context.MessageId, "An error occured during downloading", ct);
        }
        
        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
        await sessionService.CompleteSessionAsync(context, ct);
        
        _logger.LogInformation("Finished download task");
    }
}