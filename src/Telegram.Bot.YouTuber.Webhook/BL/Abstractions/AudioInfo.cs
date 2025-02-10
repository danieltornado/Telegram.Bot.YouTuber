namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions;

public sealed class AudioInfo
{
    public required string Title { get; set; }
    public required string Format { get; set; }
    public required string Quality { get; set; }
    public required string FileExtension { get; set; }
    public required string InternalUrl { get; set; }
    public required long? ContentLength { get; set; }
}