using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Extensions;

public static class SessionContextExtensions
{
    public static bool HasVideo(this SessionContext context)
    {
        return context.VideoId.HasValue;
    }

    public static bool HasAudio(this SessionContext context)
    {
        return context.AudioId.HasValue;
    }
}