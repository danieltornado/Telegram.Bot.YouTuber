using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Queues;

/// <summary>
/// Manager that accepts and holds downloading sessions until somebody picks them up  
/// </summary>
public interface ISessionsQueueService
{
    ValueTask QueueAsync(SessionContext context, CancellationToken ct);
    ValueTask<SessionContext> DequeueAsync(CancellationToken ct);
}