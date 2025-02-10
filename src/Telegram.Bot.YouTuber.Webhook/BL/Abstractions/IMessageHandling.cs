using Telegram.Bot.Types;

namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions;

public interface IMessageHandling
{
    Task HandleMessageAsync(Update update, RequestContext requestContext, CancellationToken ct);
}