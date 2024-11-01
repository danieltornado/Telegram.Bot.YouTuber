using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.YouTuber.Webhook.Services;

public interface ITelegramService
{
    Task SendWelcomeMessageAsync(long? chatId, CancellationToken ct);
    Task SendMessageAsync(long? chatId, int? replyToMessageId, string text, CancellationToken ct);
    Task SendKeyboardAsync(long? chatId, int? replyToMessageId, string text, IReplyMarkup replyMarkup, CancellationToken ct);
    Task SendInternalServerErrorAsync(long? chatId, int? replyToMessageId, Exception? exception, CancellationToken ct);
}