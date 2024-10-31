using System.Text;
using Telegram.Bot.YouTuber.Webhook.Services.Downloading;

namespace Telegram.Bot.YouTuber.Webhook.Extensions;

public static class DownloadingContextExtensions
{
    public static string GetTitleWithExtension(this DownloadingContext context)
    {
        string extension = context.GetExtensionWithPoint();

        string?[] data = [context.VideoFormat, context.VideoQuality, context.AudioFormat, context.AudioQuality];

        StringBuilder sb = new();
        foreach (var s in data)
        {
            if (!string.IsNullOrEmpty(s))
            {
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(s);
            }
        }

        var title = context.GetTitle();
        if (sb.Length > 0)
        {
            return $"{title} ({sb}){extension}";
        }

        return $"{title}{extension}";
    }

    private static string GetExtensionWithPoint(this DownloadingContext context)
    {
        var extension = context.GetExtension();
        
        if (string.IsNullOrWhiteSpace(extension))
            return ".unknown";

        if (extension.StartsWith("."))
            return extension;

        return "." + extension;
    }
    
    private static string GetTitle(this DownloadingContext context)
    {
        return context.VideoTitle ?? context.AudioTitle ?? "Unknown";
    }

    private static string GetExtension(this DownloadingContext context)
    {
        if (!string.IsNullOrWhiteSpace(context.VideoExtension))
            return context.VideoExtension;

        if (!string.IsNullOrWhiteSpace(context.VideoFormat))
            return context.VideoFormat.ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(context.AudioExtension))
            return context.AudioExtension;

        if (!string.IsNullOrWhiteSpace(context.AudioFormat))
            return context.AudioFormat.ToLowerInvariant();

        return "unknown";
    }
}