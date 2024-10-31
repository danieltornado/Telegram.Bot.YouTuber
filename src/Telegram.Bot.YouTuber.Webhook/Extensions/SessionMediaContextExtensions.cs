using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Extensions;

public static class SessionMediaContextExtensions
{
    public static string GetQuestionButtonCaption(this SessionMediaContext context)
    {
        if (context.IsSkipped)
            return "SKIP";

        return $"{context.Format?.ToUpperInvariant()}  {context.Quality}";
    }

    public static string GetTitle(this SessionMediaContext context)
    {
        return context.Title ?? "Unknown";
    }

    public static string GetQuality(this SessionMediaContext context)
    {
        return context.Quality ?? "unknown";
    }

    public static string GetFormat(this SessionMediaContext context)
    {
        return context.Format ?? "unknown";
    }

    public static string GetExtension(this SessionMediaContext context)
    {
        if (!string.IsNullOrWhiteSpace(context.Extension))
            return context.Extension;

        if (!string.IsNullOrWhiteSpace(context.Format))
            return context.Format.ToLowerInvariant();

        return "unknown";
    }
}