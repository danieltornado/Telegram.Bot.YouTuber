namespace Telegram.Bot.YouTuber.Webhook.Services;

public interface IContext
{
    public bool IsSuccess { get; }
    public Exception? Error { get; }
}