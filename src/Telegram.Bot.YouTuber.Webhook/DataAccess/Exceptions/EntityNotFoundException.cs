namespace Telegram.Bot.YouTuber.Webhook.DataAccess.Exceptions;

public sealed class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) 
        : base(message)
    {
        
    }
}