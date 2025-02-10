using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Downloading;

public interface IDownloadingClient
{
    Task<Guid> DownloadAsync(Guid sessionId, SessionMediaContext video, SessionMediaContext audio, CancellationToken ct);
}