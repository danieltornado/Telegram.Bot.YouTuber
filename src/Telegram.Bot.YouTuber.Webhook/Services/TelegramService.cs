using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.YouTuber.Webhook.Services;

internal sealed class TelegramService : ITelegramService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<TelegramService> _logger;

    public TelegramService(ITelegramBotClient botClient, ILogger<TelegramService> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }
    
    #region Implementation of ITelegramService

    public async Task SendMessageAsync(long? chatId, int? replyToMessageId, string text, CancellationToken ct)
    {
        if (!chatId.HasValue)
        {
            _logger.LogWarning("No chatId specified");
            return;
        }

        try
        {
            await _botClient.SendTextMessageAsync(chatId: chatId, text: text, parseMode: ParseMode.Html, replyToMessageId: replyToMessageId, cancellationToken: ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured while sending text message");
        }
    }

    public async Task SendKeyboardAsync(long? chatId, int? replyToMessageId, string text, IReplyMarkup replyMarkup, CancellationToken ct)
    {
        if (!chatId.HasValue)
        {
            _logger.LogWarning("No chatId specified");
            return;
        }

        try
        {
            await _botClient.SendTextMessageAsync(chatId: chatId, text: text, replyMarkup: replyMarkup, parseMode: ParseMode.Html, replyToMessageId: replyToMessageId, cancellationToken: ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured while sending keyboard message");
        }
    }

    public async Task SendInternalServerErrorAsync(long? chatId, int? replyToMessageId, Exception? exception, CancellationToken ct)
    {
        if (!chatId.HasValue)
        {
            _logger.LogWarning("No chatId specified");
            return;
        }

        string text = exception switch
        {
            VideoLibrary.Exceptions.UnavailableStreamException => "Video is not accessible",
            _ => "Internal server error"
        };

        try
        {
            await _botClient.SendTextMessageAsync(chatId: chatId, text: text, parseMode: ParseMode.Html, replyToMessageId: replyToMessageId, cancellationToken: ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured while sending keyboard message");
        }
    }

    #endregion
}