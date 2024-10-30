namespace Telegram.Bot.YouTuber.Webhook.Services.Sessions;

public sealed class SessionMediaContext
{
    public Guid Id { get; init; }
    public required int Num { get; init; }
    public required string? Title { get; init; }
    public required string InternalUrl { get; init; }
    public required string Quality { get; init; }
    public required string Format { get; init; }
    public required string Extension { get; init; }
}