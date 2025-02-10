namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Questions;

public sealed class QuestionButton
{
    public required string Caption { get; init; }
    public required string Data { get; init; }
}