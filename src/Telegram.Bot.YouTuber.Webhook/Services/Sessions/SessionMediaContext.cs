namespace Telegram.Bot.YouTuber.Webhook.Services.Sessions;

public sealed class SessionMediaContext
{
    public Guid Id { get; init; }
    public required int Num { get; init; }
    public string? Title { get; init; }
    public string? InternalUrl { get; init; }
    public string? Quality { get; init; }
    public string? Format { get; init; }
    public string? Extension { get; init; }
    public required bool IsSkipped { get; init; }
}