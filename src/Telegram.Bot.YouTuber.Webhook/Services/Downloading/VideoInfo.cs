namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

public sealed class VideoInfo
{
    public required string Title { get; set; }
    public required string Format { get; set; }
    public required string Quality { get; set; }
    public required string FileExtension { get; set; }
    public required string InternalUrl { get; set; }
    public required long? ContentLength { get; set; }
}