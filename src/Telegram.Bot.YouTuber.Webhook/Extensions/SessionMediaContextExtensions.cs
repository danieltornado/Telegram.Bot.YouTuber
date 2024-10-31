using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Extensions;

public static class SessionMediaContextExtensions
{
    public static string GetQuestionButtonCaption(this SessionMediaContext context)
    {
        if (context.IsSkipped)
            return "SKIP";

        string format = context.Format?.ToUpperInvariant() ?? string.Empty;
        string quality = context.Quality ?? string.Empty;
        string contentLength = context.ContentLength.GetValueOrDefault().BytesToHuman();

        return $"{format} | {quality} | {contentLength}";
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