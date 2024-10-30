using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;
using Telegram.Bot.YouTuber.Webhook.Extensions;
using Telegram.Bot.YouTuber.Webhook.Services;
using Telegram.Bot.YouTuber.Webhook.Services.Downloading;
using Telegram.Bot.YouTuber.Webhook.Services.Messaging;
using Telegram.Bot.YouTuber.Webhook.Services.Questions;
using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Controllers;

[ApiController]
[Route("api/message")]
public sealed class MessageController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IQuestionQueueService _questionQueueService;
    private readonly IMessageService _messageService;
    private readonly ITelegramService _telegramService;
    private readonly ILogger<MessageController> _logger;

    public MessageController(
        ISessionService sessionService,
        IQuestionQueueService questionQueueService,
        IMessageService messageService,
        ITelegramService telegramService,
        ILogger<MessageController> logger)
    {
        _sessionService = sessionService;
        _questionQueueService = questionQueueService;
        _messageService = messageService;
        _telegramService = telegramService;
        _logger = logger;
    }

    [HttpPost("update")]
    public async Task<IActionResult> HandleMessage([FromBody] Update update, CancellationToken ct)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                return await StartNewSession(update, ct);
            case UpdateType.CallbackQuery:
                return await ContinueSession(update, ct);
        }

        return Ok();
    }

    private async Task<IActionResult> StartNewSession(Update update, CancellationToken ct)
    {
        var context = await _sessionService.StartSessionAsync(update, ct);
        if (!context.IsSuccess)
        {
            _logger.LogError(context.Error, $"Failed to start session: {context.Id}");

            await _sessionService.CompleteSessionAsync(context, ct);

            await _telegramService.SendMessageAsync(context.ChatId, context.MessageId, "Internal server error", ct);

            return Ok();
        }

        return await SendQuestionTask(context, ct);
    }

    private async Task<IActionResult> ContinueSession(Update update, CancellationToken ct)
    {
        var callbackQueryContext = await _messageService.ParseCallbackQueryAsync(update, ct);
        if (!callbackQueryContext.IsSuccess)
        {
            _logger.LogError(callbackQueryContext.Error, $"Failed to parse a callbackQuery: {callbackQueryContext.Data}");

            await _telegramService.SendMessageAsync(callbackQueryContext.ChatId, callbackQueryContext.MessageId, "An incomprehensible message", ct);

            return Ok();
        }

        var context = await _sessionService.ReadSessionAsync(callbackQueryContext.SessionId, update, ct);
        if (!context.IsSuccess)
        {
            _logger.LogError(context.Error, $"Failed to read a session: {callbackQueryContext.SessionId}");

            await _sessionService.CompleteSessionAsync(context, ct);

            await _telegramService.SendMessageAsync(context.ChatId, context.MessageId, "Internal server error", ct);

            return Ok();
        }

        if (callbackQueryContext.Type == MediaType.Video)
        {
            context.SetVideoAnswer(callbackQueryContext);
            return await SendQuestionTask(context, ct);
        }

        if (callbackQueryContext.Type == MediaType.Audio)
        {
            context.SetAudioAnswer(callbackQueryContext);
            return await SendQuestionTask(context, ct);
        }

        _logger.LogError("Unknown CallbackQuery");
        await _telegramService.SendMessageAsync(context.ChatId, context.MessageId, "Internal server error", ct);

        return Ok();
    }

    private async Task<IActionResult> SendQuestionTask(SessionContext context, CancellationToken ct)
    {
        context.Scheme = HttpContext.Request.Scheme;
        context.Host = HttpContext.Request.Host;
        context.PathBase = HttpContext.Request.PathBase;

        await _questionQueueService.QueueAsync(context, ct);

        return Ok();
    }
}