using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;

namespace Telegram.Bot.YouTuber.Webhook.Services.Questions;

public sealed class QuestionData
{
    public MediaType Type { get; init; }
    public Guid MediaId { get; init; }

    public string ToCallbackQueryData()
    {
        return $"{Type}:{MediaId}";
    }

    public static QuestionData FromCallbackQueryData(string data)
    {
        var items = data.Split(':');
        if (items.Length != 2)
            throw new InvalidOperationException("Invalid data");

        if (Enum.TryParse(items[0], out MediaType mediaType))
        {
            if (Guid.TryParse(items[1], out Guid mediaId))
            {
                return new QuestionData
                {
                    Type = mediaType,
                    MediaId = mediaId
                };
            }
        }

        throw new InvalidOperationException("Invalid data");
    }
}