﻿namespace Telegram.Bot.YouTuber.Webhook.Services.Sessions;

public sealed class ContinueSessionContext
{
    public long? ChatId { get; set; }
    public int? MessageId { get; set; }
    public string? Json { get; set; }
}