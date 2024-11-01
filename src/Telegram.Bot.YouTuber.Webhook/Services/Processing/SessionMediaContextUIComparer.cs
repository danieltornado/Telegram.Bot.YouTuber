using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Processing;

public sealed class SessionMediaContextUIComparer : Comparer<SessionMediaContext>
{
    #region Overrides of Comparer<SessionMediaContext>

    public override int Compare(SessionMediaContext? x, SessionMediaContext? y)
    {
        if (x is not null && y is not null)
        {
            return CompareInternal(x, y);
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        return 0;
    }

    #endregion

    private int CompareInternal(SessionMediaContext x, SessionMediaContext y)
    {
        // asc
        int result = Comparer<bool>.Default.Compare(x.IsSkipped, y.IsSkipped);
        if (result != 0)
            return result;

        string qualityX = (x.Quality ?? string.Empty).PadLeft(4, '0');
        string qualityY = (y.Quality ?? string.Empty).PadLeft(4, '0');

        // desc
        result = string.Compare(qualityX, qualityY, StringComparison.InvariantCultureIgnoreCase);
        if (result != 0)
            return -result;
        
        // asc
        result = string.Compare(x.Format, y.Format, StringComparison.InvariantCultureIgnoreCase);
        if (result != 0)
            return result;
        
        // desc
        return -Comparer<long>.Default.Compare(x.ContentLength ?? 0, y.ContentLength ?? 0);
    }
}