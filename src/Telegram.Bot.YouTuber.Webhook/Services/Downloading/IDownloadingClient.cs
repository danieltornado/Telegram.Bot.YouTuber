using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

public interface IDownloadingClient
{
    Task<Guid> DownloadAsync(Guid sessionId, SessionMediaContext video, SessionMediaContext audio, CancellationToken ct);
}