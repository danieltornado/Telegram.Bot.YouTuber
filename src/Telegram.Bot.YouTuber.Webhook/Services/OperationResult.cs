namespace Telegram.Bot.YouTuber.Webhook.Services;

public sealed class OperationResult<T>
{
    public T? Result { get; set; }
    public bool IsSuccess { get; set; }
    public Exception? Exception { get; set; }
}