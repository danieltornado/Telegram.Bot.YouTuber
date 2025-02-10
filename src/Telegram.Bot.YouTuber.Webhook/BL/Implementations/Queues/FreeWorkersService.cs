using System.Threading.Channels;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Downloading;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Queues;
using Telegram.Bot.YouTuber.Webhook.BL.Implementations.Downloading;
using Telegram.Bot.YouTuber.Webhook.Extensions;

namespace Telegram.Bot.YouTuber.Webhook.BL.Implementations.Queues;

/// <inheritdoc cref="IFreeWorkersService" />
internal sealed class FreeWorkersService : IFreeWorkersService
{
    private readonly ILogger<WorkerInstance> _logger;
    private readonly Channel<IWorkerInstance> _queue;

    private int _counter;

    public FreeWorkersService(IConfiguration configuration, ILogger<WorkerInstance> logger)
    {
        _logger = logger;
        _queue = Channel.CreateBounded<IWorkerInstance>(configuration.GetWorkersCount());
    }

    #region Implementation of IWorkersQueueService

    /// <inheritdoc />
    public ValueTask QueueAsync(IWorkerInstance instance, CancellationToken ct)
    {
        _logger.LogInformation("Queueing worker instance: {WorkerId}", instance.WorkerId);
        return _queue.Writer.WriteAsync(instance, ct);
    }

    /// <inheritdoc />
    public ValueTask<IWorkerInstance> DequeueAsync(CancellationToken ct)
    {
        return _queue.Reader.ReadAsync(ct);
    }

    /// <inheritdoc />
    public int GenerateUniqueWorkerId()
    {
        return Interlocked.Increment(ref _counter);
    }

    #endregion
}