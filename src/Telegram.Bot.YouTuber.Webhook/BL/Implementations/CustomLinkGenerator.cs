using Telegram.Bot.YouTuber.Webhook.BL.Abstractions;

namespace Telegram.Bot.YouTuber.Webhook.BL.Implementations;

public sealed class CustomLinkGenerator : ICustomLinkGenerator
{
    #region Implementation of ICustomLinkGenerator

    /// <inheritdoc />
    public string GenerateFileLink(Guid fileId, RequestContext? requestContext)
    {
        if (requestContext is null)
            return "couldn't generate link";
        
        return $"{requestContext.Scheme}://{requestContext.Host}/{requestContext.PathFile}/{fileId}";
    }

    #endregion
}