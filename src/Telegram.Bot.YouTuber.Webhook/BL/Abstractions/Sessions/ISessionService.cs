using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;

namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;

/// <summary>
/// Service works with <see cref="SessionContext"/> through db
/// </summary>
public interface ISessionService
{
    Task<SessionContext> StartSessionAsync(StartSessionContext context, CancellationToken ct);
    Task<List<SessionMediaContext>> SaveMediaAsync(Guid sessionId, IReadOnlyCollection<VideoInfo> video, IReadOnlyCollection<AudioInfo> audio, CancellationToken ct);
    Task CompleteSessionAsync(Guid sessionId, CancellationToken ct);
    Task SetFailedSessionAsync(Guid sessionId, string error, CancellationToken ct);
    Task<SessionContext> GetSessionByMediaAsync(Guid mediaId, CancellationToken ct);
    Task<List<SessionMediaContext>> GetMediaBySessionAsync(Guid sessionId, MediaType mediaType, CancellationToken ct);
    Task SaveSessionAnswerAsync(Guid mediaId, string? json, CancellationToken ct);
    Task<List<SessionMediaContext>> GetMediaAsync(IEnumerable<Guid> mediaIds, CancellationToken ct);
}