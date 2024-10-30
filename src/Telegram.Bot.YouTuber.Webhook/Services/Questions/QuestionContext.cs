namespace Telegram.Bot.YouTuber.Webhook.Services.Questions;

public sealed class QuestionContext
{
    public string? Title { get; set; }
    public List<QuestionButton>? Buttons { get; set; }

    public bool IsSuccess { get; set; }
    public Exception? Error { get; set; }
}