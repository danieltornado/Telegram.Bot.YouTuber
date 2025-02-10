using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.BL.Implementations.Sessions;

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