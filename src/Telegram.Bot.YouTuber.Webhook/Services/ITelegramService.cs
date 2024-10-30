using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.YouTuber.Webhook.Services;

public interface ITelegramService
{
    Task SendMessageAsync(long? chatId, int? replyToMessageId, string text, CancellationToken ct);
    Task SendKeyboardAsync(long? chatId, int? replyToMessageId, string text, IReplyMarkup replyMarkup, CancellationToken ct);
}