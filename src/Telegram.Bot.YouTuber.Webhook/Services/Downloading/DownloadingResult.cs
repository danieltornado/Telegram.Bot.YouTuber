namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

public sealed class DownloadingResult
{
    public Guid FileId { get; set; }
    public bool IsSkipped { get; set; }
}