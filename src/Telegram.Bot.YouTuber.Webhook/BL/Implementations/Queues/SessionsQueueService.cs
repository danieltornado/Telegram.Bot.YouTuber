using System.Threading.Channels;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Queues;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.BL.Implementations.Queues;

/// <inheritdoc cref="ISessionsQueueService" />
internal sealed class SessionsQueueService : ISessionsQueueService
{
    private readonly Channel<SessionContext> _queue;

    public SessionsQueueService()
    {
        _queue = Channel.CreateUnbounded<SessionContext>();
    }

    #region Implementation of IActionQueueService

    /// <inheritdoc />
    public ValueTask QueueAsync(SessionContext context, CancellationToken ct)
    {
        return _queue.Writer.WriteAsync(context, ct);
    }

    /// <inheritdoc />
    public ValueTask<SessionContext> DequeueAsync(CancellationToken ct)
    {
        return _queue.Reader.ReadAsync(ct);
    }

    #endregion
}