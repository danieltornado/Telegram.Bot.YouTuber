namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;

public sealed class StartSessionContext
{
    public long? ChatId { get; set; }
    public int? MessageId { get; set; }
    public string? Url { get; set; }
    public string? Json { get; set; }
}