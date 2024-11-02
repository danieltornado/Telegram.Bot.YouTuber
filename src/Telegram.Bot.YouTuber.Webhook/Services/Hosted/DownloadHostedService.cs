using Telegram.Bot.YouTuber.Webhook.DataAccess.Exceptions;
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
            catch (TaskCanceledException e)
            {
                _logger.LogError(e, "Download service cancelled");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Download service failed");
            }
        }
    }

    #endregion

    private async Task DoWorkAsync(CancellationToken ct)
    {
        var context = await _downloadQueueService.DequeueAsync(ct);

        _logger.LogInformation("Started download task");

        using var scope = _serviceProvider.CreateScope();

        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
        var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramService>();
        var downloadingClient = scope.ServiceProvider.GetRequiredService<IDownloadingClient>();

        try
        {
            var list = await sessionService.GetMediaAsync([context.VideoId.GetValueOrDefault(), context.AudioId.GetValueOrDefault()], ct);

            var video = list.FirstOrDefault(e => e.Id == context.VideoId);
            if (video is null)
                throw new EntityNotFoundException("Media not found");

            var audio = list.FirstOrDefault(e => e.Id == context.AudioId);
            if (audio is null)
                throw new EntityNotFoundException("Media not found");

            if (video.IsSkipped && audio.IsSkipped)
            {
                await telegramService.SendMessageAsync(context.ChatId, context.MessageId, "Nothing to download", ct);
            }
            else
            {
                await telegramService.SendMessageAsync(context.ChatId, context.MessageId, "Processing...", ct);

                using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
                tokenSource.CancelAfter(TimeSpan.FromHours(2));

                var fileId = await downloadingClient.DownloadAsync(context.Id, video, audio, tokenSource.Token);

                var linkGenerator = scope.ServiceProvider.GetRequiredService<LinkGenerator>();
                var link = linkGenerator.GenerateFileLink(fileId, context.RequestContext);
                if (link is null)
                {
                    await telegramService.SendMessageAsync(context.ChatId, context.MessageId, "An error occured during generating link", ct);
                }
                else
                {
                    await telegramService.SendMessageAsync(context.ChatId, context.MessageId, link, ct);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Download service failed");

            await sessionService.SetFailedSessionAsync(context.Id, e.ToString(), ct);

            await telegramService.SendMessageAsync(context.ChatId, context.MessageId, "An error occured during processing", ct);
        }

        await sessionService.CompleteSessionAsync(context.Id, ct);

        _logger.LogInformation("Finished download task");
    }
}