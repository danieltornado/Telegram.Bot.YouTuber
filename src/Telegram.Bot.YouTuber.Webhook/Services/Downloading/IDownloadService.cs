using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

public interface IDownloadService
{
    Task DownloadAsync(SessionContext context, CancellationToken ct);
}