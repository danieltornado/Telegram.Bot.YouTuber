namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

public sealed class VideoInfo
{
    public required string Title { get; init; }
    public required string Format { get; init; }
    public required string Quality { get; init; }
    public required string FileExtension { get; init; }
    public required string InternalUrl { get; init; }
}