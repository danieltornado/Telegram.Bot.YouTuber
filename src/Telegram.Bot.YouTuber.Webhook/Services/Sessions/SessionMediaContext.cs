namespace Telegram.Bot.YouTuber.Webhook.Services.Sessions;

public sealed class SessionMediaContext
{
    public Guid Id { get; set; }
    public int Num { get; set; }
    public string? Title { get; set; }
    public string? InternalUrl { get; set; }
    public string? Quality { get; set; }
    public string? Format { get; set; }
    public string? Extension { get; set; }
    public long? ContentLength { get; set; }
    public bool IsSkipped { get; set; }
}