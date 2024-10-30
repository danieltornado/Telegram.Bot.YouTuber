using System.Threading.Channels;
using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

public sealed class DownloadQueueService : IDownloadQueueService
{
    private readonly Channel<SessionContext> _queue;

    public DownloadQueueService()
    {
        _queue = Channel.CreateUnbounded<SessionContext>();
    }
    
    #region Implementation of IActionQueueService

    public async Task QueueAsync(SessionContext context, CancellationToken ct)
    {
        await _queue.Writer.WriteAsync(context, ct);
    }

    public async Task<SessionContext> DequeueAsync(CancellationToken ct)
    {
        return await _queue.Reader.ReadAsync(ct);
    }

    #endregion
}