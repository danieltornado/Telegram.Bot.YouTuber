namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;

/// <summary>
/// Service works with <see cref="DownloadingContext" /> through db
/// </summary>
public interface IDownloadingService
{
    Task<Guid> StartDownloadingAsync(Guid sessionId, SessionMediaContext video, SessionMediaContext audio, CancellationToken ct);
    Task CompleteDownloadingAsync(Guid downloadingId, CancellationToken ct);
    Task<DownloadingContext?> GetDownloadingAsync(Guid downloadingId, CancellationToken ct);
    Task SetFailedDownloadingAsync(Guid downloadingId, string error, CancellationToken ct);
    Task UpdateDownloadingVideoExtensionAsync(Guid downloadingId, string videoExtension, CancellationToken ct);
}