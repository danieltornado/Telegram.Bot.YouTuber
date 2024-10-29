namespace Telegram.Bot.YouTuber.Core.Extensions;

public static class ObjectExtensions
{
    public static T AsNotNull<T>(this T? obj, string? paramName = null, string? message = null)
        where T : class
    {
        return obj ?? throw new ArgumentNullException(paramName, message);
    }
}