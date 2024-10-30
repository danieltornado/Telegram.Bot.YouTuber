using System.Threading.Channels;
using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Questions;

public sealed class QuestionQueueService : IQuestionQueueService
{
    private readonly Channel<SessionContext> _queue;

    public QuestionQueueService()
    {
        _queue = Channel.CreateUnbounded<SessionContext>();
    }
    
    #region Implementation of IQuestionQueueService

    public async Task QueueAsync(SessionContext context, CancellationToken ct)
    {
        await _queue.Writer.WriteAsync(context, ct);
    }

    public async Task<SessionContext> DequeueAsync(CancellationToken ct)
    {
        return await _queue.Reader.ReadAsync(ct);
    }

    #endregion
}