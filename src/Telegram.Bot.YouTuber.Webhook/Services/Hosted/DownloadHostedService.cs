using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Queues;

namespace Telegram.Bot.YouTuber.Webhook.Services.Hosted;

public sealed class DownloadHostedService : BackgroundService
{
    private readonly IFreeWorkersService _freeWorkerService;
    private readonly ISessionsQueueService _sessionsQueueService;
    private readonly ILogger<DownloadHostedService> _logger;

    public DownloadHostedService(
        IFreeWorkersService freeWorkerService,
        ISessionsQueueService sessionsQueueService,
        ILogger<DownloadHostedService> logger)
    {
        _freeWorkerService = freeWorkerService;
        _sessionsQueueService = sessionsQueueService;
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
                _logger.LogError(e, "Download service task-cancelled");
                break;
            }
            catch (OperationCanceledException e)
            {
                _logger.LogError(e, "Download service operation-cancelled");
                break;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Download service failed");
                break;
            }
        }
    }

    #endregion

    private async Task DoWorkAsync(CancellationToken ct)
    {
        // Waiting a free worker
        var worker = await _freeWorkerService.DequeueAsync(ct);

        _logger.LogInformation("Worker has been dequeued: {WorkerId}", worker.WorkerId);

        // Waiting a next session
        var context = await _sessionsQueueService.DequeueAsync(ct);

        _logger.LogInformation("Session has been dequeued");

        _ = worker.ProcessAsync(context, ct);
    }
}