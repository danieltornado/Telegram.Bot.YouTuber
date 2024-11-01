using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;

namespace Telegram.Bot.YouTuber.Webhook.Services.Sessions;

public sealed class SessionMediaContext
{
    public Guid Id { get; set; }
    public MediaType Type { get; set; }
    public string? Title { get; set; }
    public string? InternalUrl { get; set; }
    public string? Quality { get; set; }
    public string? Format { get; set; }
    public string? Extension { get; set; }
    public long? ContentLength { get; set; }
    public bool IsSkipped { get; set; }
}