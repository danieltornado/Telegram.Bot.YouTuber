using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Downloading;

/// <summary>
/// The instance downloads a session
/// </summary>
public interface IWorkerInstance
{
    /// <summary>
    /// Download a session
    /// </summary>
    /// <param name="context"></param>
    /// <param name="ct"></param>
    public Task ProcessAsync(SessionContext context, CancellationToken ct);

    /// <summary>
    /// Gets worker id
    /// </summary>
    int WorkerId { get; }
}