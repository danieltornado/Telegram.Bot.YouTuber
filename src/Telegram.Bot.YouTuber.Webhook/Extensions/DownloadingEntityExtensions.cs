using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;

namespace Telegram.Bot.YouTuber.Webhook.Extensions;

public static class DownloadingEntityExtensions
{
    public static string GetTitle(this DownloadingEntity entity)
    {
        return entity.VideoTitle ?? entity.AudioTitle ?? "Unknown";
    }

    public static string GetExtension(this DownloadingEntity entity)
    {
        if (!string.IsNullOrWhiteSpace(entity.VideoExtension))
            return entity.VideoExtension;

        if (!string.IsNullOrWhiteSpace(entity.VideoFormat))
            return entity.VideoFormat.ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(entity.AudioExtension))
            return entity.AudioExtension;

        if (!string.IsNullOrWhiteSpace(entity.AudioFormat))
            return entity.AudioFormat.ToLowerInvariant();

        return "unknown";
    }
}