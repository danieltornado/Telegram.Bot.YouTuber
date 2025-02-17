namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions;

public interface ICustomLinkGenerator
{
    string GenerateFileLink(Guid fileId, RequestContext? requestContext);
}