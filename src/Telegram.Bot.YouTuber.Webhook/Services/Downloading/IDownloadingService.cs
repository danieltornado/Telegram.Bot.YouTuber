using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

public interface IDownloadingService
{
    Task<Guid> StartDownloadingAsync(SessionContext sessionContext, SessionMediaContext video, SessionMediaContext audio, CancellationToken ct);
    Task CompleteDownloadingAsync(DownloadingContext context, CancellationToken ct);
    Task<DownloadingContext> GetDownloadingAsync(Guid downloadingId, CancellationToken ct);
}