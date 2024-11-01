﻿using Telegram.Bot.YouTuber.Webhook.Services;

namespace Telegram.Bot.YouTuber.Webhook.Extensions;

public static class LinkGeneratorExtensions
{
    public static string? GenerateFileLink(this LinkGenerator linkGenerator, Guid fileId, RequestContext? requestContext)
    {
        if (requestContext is null)
            return linkGenerator.GetPathByAction("GetFile", "File", new { fileId });

        return linkGenerator.GetUriByAction("GetFile", "File", new { fileId }, scheme: requestContext.Scheme, host: requestContext.Host, pathBase: requestContext.PathBase);
    }
}