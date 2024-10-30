using Telegram.Bot.Types;

namespace Telegram.Bot.YouTuber.Webhook.Services.Messaging;

public interface IMessageService
{
    Task<CallbackQueryContext> ParseCallbackQueryAsync(Update update, CancellationToken ct);
}