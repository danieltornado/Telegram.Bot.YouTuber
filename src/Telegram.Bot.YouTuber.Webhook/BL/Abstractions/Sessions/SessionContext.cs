namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;

public sealed class SessionContext
{
    public Guid Id { get; set; }

    public long? ChatId { get; set; }

    public int? MessageId { get; set; }

    public string? Url { get; set; }

    public Guid? VideoId { get; set; }

    public Guid? AudioId { get; set; }

    /// <summary>
    /// Gets or sets context execution. Sets only in Controller
    /// </summary>
    public RequestContext? RequestContext { get; set; }
}