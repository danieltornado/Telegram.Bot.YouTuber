using Telegram.Bot.Types;

namespace Telegram.Bot.YouTuber.Webhook.Services.Processing;

public interface IMessageHandling
{
    Task HandleMessageAsync(Update update, RequestContext requestContext, CancellationToken ct);
}