namespace Telegram.Bot.YouTuber.Webhook.Extensions;

public static class EnumerableExtensions
{
    public static async Task<IReadOnlyList<T>> AsReadOnlyListAsync<T>(this Task<IEnumerable<T>?> source)
    {
        var result = await source;
        return result.AsReadOnlyList();
    }
    
    public static IReadOnlyList<T> AsReadOnlyList<T>(this IEnumerable<T>? source)
    {
        if (source is null)
            return Array.Empty<T>();

        return source as IReadOnlyList<T> ?? source.ToList();
    }
}