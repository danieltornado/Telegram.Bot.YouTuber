using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;

namespace Telegram.Bot.YouTuber.Webhook.Services.Questions;

public sealed class QuestionData
{
    public Guid SessionId { get; init; }
    public MediaType Type { get; init; }
    public int Num { get; init; }
}