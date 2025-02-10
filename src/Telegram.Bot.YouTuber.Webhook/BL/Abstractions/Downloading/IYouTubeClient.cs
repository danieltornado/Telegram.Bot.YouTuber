namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Downloading;

public interface IYouTubeClient
{
    Task DownloadAsync(string internalUrl, long? contentLength, Stream destination, CancellationToken ct);
    Task<(IReadOnlyList<VideoInfo> Video, IReadOnlyList<AudioInfo> Audio)> GetMetadataAsync(string url, CancellationToken ct);
}