using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions;
using Telegram.Bot.YouTuber.Webhook.Extensions;

namespace Telegram.Bot.YouTuber.Webhook.Controllers;

[ApiController]
[Route("api/messages")]
public sealed class MessageController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public MessageController(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    [HttpPost("update")]
    public IActionResult HandleMessage([FromBody] Update update)
    {
        RequestContext requestContext = new()
        {
            Host = Request.Host,
            Scheme = Request.Scheme,
            PathFile = _configuration.GetPathFile(),
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