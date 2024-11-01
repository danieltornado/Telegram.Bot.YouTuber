﻿using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.YouTuber.Webhook.Services;
using Telegram.Bot.YouTuber.Webhook.Services.Processing;

namespace Telegram.Bot.YouTuber.Webhook.Controllers;

[ApiController]
[Route("api/message")]
public sealed class MessageController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public MessageController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpPost("update")]
    public IActionResult HandleMessage([FromBody] Update update, CancellationToken ct)
    {
        RequestContext requestContext = new()
        {
            Host = Request.Host,
            Scheme = Request.Scheme,
            PathBase = Request.PathBase,
        };

        StartHandling(update, requestContext);
        return Ok();
    }

    private async void StartHandling(Update update, RequestContext requestContext)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<IMessageHandling>();
        await handler.HandleMessageAsync(update, requestContext, CancellationToken.None);
    }
}