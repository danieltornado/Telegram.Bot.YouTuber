using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;

namespace Telegram.Bot.YouTuber.Webhook.Services.Questions;

public sealed class QuestionData
{
    public Guid SessionId { get; init; }
    public MediaType Type { get; init; }
    public int Num { get; init; }

    public string ToCallbackQueryData()
    {
        return $"{SessionId:D}:{Type}:{Num}";
    }

    public static QuestionData FromCallbackQueryData(string data)
    {
        var items = data.Split(':');
        if (items.Length != 3)
            throw new InvalidOperationException("Invalid data");

        if (Guid.TryParse(items[0], out Guid sessionId))
        {
            if (Enum.TryParse(items[1], out MediaType mediaType))
            {
                if (int.TryParse(items[2], out int typeNum))
                {
                    return new QuestionData
                    {
                        SessionId = sessionId,
                        Num = typeNum,
                        Type = mediaType
                    };
                }
            }
        }

        throw new InvalidOperationException("Invalid data");
    }
}