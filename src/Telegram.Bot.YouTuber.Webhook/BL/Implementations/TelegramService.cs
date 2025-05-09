﻿using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions;
using Telegram.Bot.YouTuber.Webhook.DataAccess.Exceptions;

namespace Telegram.Bot.YouTuber.Webhook.BL.Implementations;

internal sealed class TelegramService : ITelegramService
{
    const string WelcomeMessage = @"<b>Welcome!</b>🙋
I'm a bot who can download a file for you from youtube.com

1. Input here an <b>url</b>
2. Select one item from the <b>video</b> list 🖥
3. Select one item from the <b>audio</b> list 🔉
4. Wait
5. Download the file 👍";

    private const string WebmAacWarningMessage = @"⚠️ <b>WEBM + AAC</b> will be converted to <b>MKV</b>";

    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<TelegramService> _logger;

    public TelegramService(ITelegramBotClient botClient, ILogger<TelegramService> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    #region Implementation of ITelegramService

    public async Task SendWelcomeMessageAsync(long? chatId, CancellationToken ct)
    {
        if (!chatId.HasValue)
        {
            _logger.LogWarning("No chatId specified");
            return;
        }

        try
        {
            await _botClient.SendMessage(chatId: chatId, text: WelcomeMessage, parseMode: ParseMode.Html, cancellationToken: ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured while sending text message");
        }
    }

    public async Task SendMessageAsync(long? chatId, int? replyToMessageId, string text, CancellationToken ct)
    {
        if (!chatId.HasValue)
        {
            _logger.LogWarning("No chatId specified");
            return;
        }

        try
        {
            await _botClient.SendMessage(chatId: chatId, text: text, parseMode: ParseMode.Html, replyParameters: replyToMessageId, cancellationToken: ct);
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
            await _botClient.SendMessage(chatId: chatId, text: text, replyMarkup: replyMarkup, parseMode: ParseMode.Html, replyParameters: replyToMessageId, cancellationToken: ct);
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
            EntityNotFoundException e => e.Message,
            _ => "Internal server error"
        };

        try
        {
            await _botClient.SendMessage(chatId: chatId, text: text, parseMode: ParseMode.Html, replyParameters: replyToMessageId, cancellationToken: ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured while sending keyboard message");
        }
    }

    /// <inheritdoc />
    public async Task SendInvalidUrlMessageAsync(long? chatId, int? replyToMessageId, CancellationToken ct)
    {
        if (!chatId.HasValue)
        {
            _logger.LogWarning("No chatId specified");
            return;
        }

        try
        {
            await _botClient.SendMessage(chatId: chatId, text: "Invalid youtube url", parseMode: ParseMode.Html, replyParameters: replyToMessageId, cancellationToken: ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured while sending text message");
        }
    }

    /// <inheritdoc />
    public async Task SendWarningWebmAacAsync(long? chatId, CancellationToken ct)
    {
        if (!chatId.HasValue)
        {
            _logger.LogWarning("No chatId specified");
            return;
        }

        try
        {
            await _botClient.SendMessage(chatId: chatId, text: WebmAacWarningMessage, parseMode: ParseMode.Html, cancellationToken: ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured while sending text message");
        }
    }

    #endregion
}