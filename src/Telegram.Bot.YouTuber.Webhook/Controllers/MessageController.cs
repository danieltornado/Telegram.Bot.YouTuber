using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.YouTuber.Webhook.Services;

namespace Telegram.Bot.YouTuber.Webhook.Controllers;

[ApiController]
[Route("api/message")]
public sealed class MessageController : ControllerBase
{
    private readonly ITelegramService _telegramService;
    private readonly ILogger<MessageController> _logger;

    public MessageController(ITelegramService telegramService, ILogger<MessageController> logger)
    {
        _telegramService = telegramService;
        _logger = logger;
    }
    
    [HttpPost("update")]
    public async Task<IActionResult> HandleMessage([FromBody] Update update, CancellationToken ct)
    {
        await _telegramService.SendMessageAsync(update.Message.Chat.Id, null, "Hello", ct);
        return Ok();
    }
}