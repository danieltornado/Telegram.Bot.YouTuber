namespace Telegram.Bot.YouTuber.Webhook.Extensions;

public static class StringExtensions
{
    public static Guid? ToGuid(this string fileId)
    {
        if (Guid.TryParse(fileId, out var result))
            return result;

        return null;
    }
}