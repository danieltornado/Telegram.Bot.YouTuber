using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Extensions;

public static class LinkGeneratorExtensions
{
    public static string? GenerateFileLink(this LinkGenerator linkGenerator, SessionContext context)
    {
        if (string.IsNullOrEmpty(context.Scheme))
            return linkGenerator.GetPathByAction("GetFile", "File", new { fileId = context.Id }); 
        
        return linkGenerator.GetUriByAction("GetFile", "File", new { fileId = context.Id }, scheme: context.Scheme, host: context.Host, pathBase: context.PathBase);
    }
}