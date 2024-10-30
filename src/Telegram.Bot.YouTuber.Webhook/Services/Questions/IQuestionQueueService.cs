using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Questions;

public interface IQuestionQueueService
{
    Task QueueAsync(SessionContext context, CancellationToken ct);
    Task<SessionContext> DequeueAsync(CancellationToken ct);
}