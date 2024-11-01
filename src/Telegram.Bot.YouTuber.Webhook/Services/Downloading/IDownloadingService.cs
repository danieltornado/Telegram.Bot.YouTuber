using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

public interface IDownloadingService
{
    Task<Guid> StartDownloadingAsync(Guid sessionId, SessionMediaContext video, SessionMediaContext audio, CancellationToken ct);
    Task CompleteDownloadingAsync(Guid downloadingId, CancellationToken ct);
    Task<DownloadingContext?> GetDownloadingAsync(Guid downloadingId, CancellationToken ct);
    Task SetFailedDownloadingAsync(Guid downloadingId, string error, CancellationToken ct);
}