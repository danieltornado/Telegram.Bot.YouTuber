using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

public interface IDownloadingClient
{
    Task<DownloadingContext> DownloadAsync(SessionContext sessionContext, CancellationToken ct);
}