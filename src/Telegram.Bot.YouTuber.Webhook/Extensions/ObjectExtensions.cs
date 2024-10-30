namespace Telegram.Bot.YouTuber.Webhook.Extensions;

public static class ObjectExtensions
{
    public static Task<T> AsTask<T>(this T value)
    {
        return Task.FromResult(value);
    }
}