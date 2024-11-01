namespace Telegram.Bot.YouTuber.Webhook.Services.Sessions;

public sealed class SessionContext
{
    public Guid Id { get; set; }
    
    public long? ChatId { get; set; }
    
    public int? MessageId { get; set; }
    
    public string? Url { get; set; }
    
    public Guid? VideoId { get; set; }
    
    public Guid? AudioId { get; set; }
    
    #region Services

    public RequestContext? RequestContext { get; set; }

    #endregion
}