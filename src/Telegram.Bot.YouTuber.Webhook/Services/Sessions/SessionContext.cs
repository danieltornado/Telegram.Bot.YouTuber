namespace Telegram.Bot.YouTuber.Webhook.Services.Sessions;

public sealed class SessionContext : IContext
{
    public Guid Id { get; set; }
    public long? ChatId { get; set; }
    public int? MessageId { get; set; }
    public string? Url { get; set; }

    public string? Json { get; set; }
    public string? JsonVideo { get; set; }
    public string? JsonAudio { get; set; }

    public Guid? VideoId { get; set; }
    public Guid? AudioId { get; set; }

    public List<SessionMediaContext> Videos { get; } = new();

    public List<SessionMediaContext> Audios { get; } = new();

    public string? Scheme { get; set; }
    public PathString PathBase { get; set; }
    public HostString Host { get; set; }
    
    public Guid? FileId { get; set; }

    #region Implementation of IContext

    public bool IsSuccess { get; set; }
    public Exception? Error { get; set; }

    #endregion
}