using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Downloading;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Queues;
using Telegram.Bot.YouTuber.Webhook.Extensions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Hosted;

/// <summary>
/// Service registers workers which will process downloading sessions
/// </summary>
public sealed class WorkersRegistrationHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly IFreeWorkersService _freeWorkersService;
    private readonly ILogger<WorkersRegistrationHostedService> _logger;

    public WorkersRegistrationHostedService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        IFreeWorkersService freeWorkersService,
        ILogger<WorkersRegistrationHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _freeWorkersService = freeWorkersService;
        _logger = logger;
    }

    #region Overrides of BackgroundService

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Registering workers...");

        int workersCount = _configuration.GetWorkersCount();
        for (int i = 0; i < workersCount; i++)
        {
            var instance = _serviceProvider.GetRequiredService<IWorkerInstance>();
            await _freeWorkersService.QueueAsync(instance, stoppingToken);
        }

        _logger.LogInformation("Registering workers is completed");
    }

    #endregion
}