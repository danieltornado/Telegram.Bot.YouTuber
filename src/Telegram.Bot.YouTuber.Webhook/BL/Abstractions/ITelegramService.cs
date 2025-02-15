using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions;

public interface ITelegramService
{
    Task SendWelcomeMessageAsync(long? chatId, CancellationToken ct);
    Task SendMessageAsync(long? chatId, int? replyToMessageId, string text, CancellationToken ct);
    Task SendKeyboardAsync(long? chatId, int? replyToMessageId, string text, IReplyMarkup replyMarkup, CancellationToken ct);
    Task SendInternalServerErrorAsync(long? chatId, int? replyToMessageId, Exception? exception, CancellationToken ct);

    /// <summary>
    /// Sends the text message "Invalid url" to a user
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="replyToMessageId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task SendInvalidUrlMessageAsync(long? chatId, int? replyToMessageId, CancellationToken ct);
}