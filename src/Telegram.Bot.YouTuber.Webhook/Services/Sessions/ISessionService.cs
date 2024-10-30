using Telegram.Bot.Types;

namespace Telegram.Bot.YouTuber.Webhook.Services.Sessions;

public interface ISessionService
{
    Task<SessionContext> StartSessionAsync(Update update, CancellationToken ct);
    Task<SessionContext> ReadSessionAsync(Guid id, Update update, CancellationToken ct);
    Task SaveSessionAsync(SessionContext sessionContext, CancellationToken ct);
    Task CompleteSessionAsync(SessionContext sessionContext, CancellationToken ct);
    Task<string?> GetTitleAsync(Guid id, CancellationToken ct);
}