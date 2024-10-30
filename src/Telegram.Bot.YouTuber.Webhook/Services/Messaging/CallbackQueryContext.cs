using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;

namespace Telegram.Bot.YouTuber.Webhook.Services.Messaging;

public sealed class CallbackQueryContext
{
    public string? Data { get; set; }
    public long? ChatId { get; set; }
    public int? MessageId { get; set; }
    public Guid SessionId { get; set; }
    public MediaType Type { get; set; }
    public int Num { get; set; }
    public string? Json { get; set; }

    public bool IsSuccess { get; set; }
    public Exception? Error { get; set; }
}