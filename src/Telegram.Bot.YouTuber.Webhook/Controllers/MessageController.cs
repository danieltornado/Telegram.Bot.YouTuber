using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions;

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

        _ = StartHandling(update, requestContext);
        return Ok();
    }

    private async Task StartHandling(Update update, RequestContext requestContext)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<IMessageHandling>();
        
        using var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(TimeSpan.FromHours(2));
        await handler.HandleMessageAsync(update, requestContext, tokenSource.Token);
    }
}