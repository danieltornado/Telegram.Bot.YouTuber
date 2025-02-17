namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions;

public sealed class RequestContext
{
    /// <summary>
    /// Scheme: https://
    /// </summary>
    public required string Scheme { get; init; }
    public required string PathFile { get; init; }
    public required  HostString Host { get; init; }
}