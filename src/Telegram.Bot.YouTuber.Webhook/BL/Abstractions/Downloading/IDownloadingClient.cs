using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Downloading;

public interface IDownloadingClient
{
    Task<Guid> DownloadAsync(SessionContext session, SessionMediaContext video, SessionMediaContext audio, CancellationToken ct);
}