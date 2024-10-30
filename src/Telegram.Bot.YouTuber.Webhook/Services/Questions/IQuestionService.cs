using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Questions;

public interface IQuestionService
{
    Task<QuestionContext> GetVideoQuestionAsync(SessionContext sessionContext, CancellationToken ct);
    Task<QuestionContext> GetAudioQuestionAsync(SessionContext sessionContext, CancellationToken ct);
}