using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Downloading;

namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Queues;

/// <summary>
/// Manager that accepts and holds workers which download session until somebody picks them up
/// </summary>
public interface IFreeWorkersService
{
    ValueTask QueueAsync(IWorkerInstance instance, CancellationToken ct);
    ValueTask<IWorkerInstance> DequeueAsync(CancellationToken ct);

    /// <summary>
    /// Generates worker id
    /// </summary>
    /// <returns></returns>
    int GenerateUniqueWorkerId();
}