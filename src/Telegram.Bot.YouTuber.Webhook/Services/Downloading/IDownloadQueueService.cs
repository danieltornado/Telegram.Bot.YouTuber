using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

public interface IDownloadQueueService
{
    Task QueueAsync(SessionContext context, CancellationToken ct);
    Task<SessionContext> DequeueAsync(CancellationToken ct);
}