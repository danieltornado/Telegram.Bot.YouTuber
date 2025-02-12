﻿namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions;

public sealed class RequestContext
{
    public required string Scheme { get; init; } = "https://";
    public required PathString PathBase { get; init; }
    public required  HostString Host { get; init; }
}