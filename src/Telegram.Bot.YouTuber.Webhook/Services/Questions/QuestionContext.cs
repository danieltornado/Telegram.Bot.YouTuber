namespace Telegram.Bot.YouTuber.Webhook.Services.Questions;

public sealed class QuestionContext : IContext
{
    public string? Title { get; set; }
    public List<QuestionButton>? Buttons { get; set; }

    #region Implementation of IContext

    public bool IsSuccess { get; set; }
    public Exception? Error { get; set; }

    #endregion
}