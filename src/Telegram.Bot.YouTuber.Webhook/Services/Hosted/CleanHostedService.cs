using Microsoft.EntityFrameworkCore;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions;
using Telegram.Bot.YouTuber.Webhook.DataAccess;

namespace Telegram.Bot.YouTuber.Webhook.Services.Hosted;

public sealed class CleanHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CleanHostedService> _logger;

    public CleanHostedService(IServiceProvider serviceProvider, ILogger<CleanHostedService> logger)
    {
        _serviceProvider = serviceProvider;
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

                await Task.Delay(TimeSpan.FromHours(3), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Clean service is cancelled");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Clean service is failed");

                if (!stoppingToken.IsCancellationRequested)
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    #endregion

    private async Task DoWorkAsync(CancellationToken ct)
    {
        _logger.LogInformation("Cleaning started");

        var minValue = DateTime.UtcNow.AddHours(-3);

        using var scope = _serviceProvider.CreateScope();
        var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var toDelete = await dbContext.Sessions.Include(e => e.Downloading).Where(e => e.UpdatedAt < minValue).ToListAsync(ct);
        foreach (var sessionToDelete in toDelete)
        {
            dbContext.Remove(sessionToDelete);

            if (sessionToDelete.Downloading != null)
            {
                foreach (var downloadingEntity in sessionToDelete.Downloading)
                {
                    dbContext.Remove(downloadingEntity);

                    await fileService.DeleteDownloadingAsync(downloadingEntity.Id, ct);
                }
            }
        }

        await dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Cleaning finished");
    }
}