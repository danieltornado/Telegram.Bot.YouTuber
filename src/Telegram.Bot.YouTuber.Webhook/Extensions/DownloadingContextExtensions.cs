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

        if (sb.Length > 0)
        {
            return $"{context.Title} ({sb}){extension}";
        }

        return $"{context.Title}{extension}";
    }

    public static string GetExtensionWithPoint(this DownloadingContext context)
    {
        if (string.IsNullOrWhiteSpace(context.Extension))
            return ".unknown";

        if (context.Extension.StartsWith("."))
            return context.Extension;

        return "." + context.Extension;
    }
}