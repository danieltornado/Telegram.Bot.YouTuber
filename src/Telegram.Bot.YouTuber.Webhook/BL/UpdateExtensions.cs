using System.Text.Json;
using Telegram.Bot.Types;

namespace Telegram.Bot.YouTuber.Webhook.BL;

public static class UpdateExtensions
{
    public static string CreateJson(this Update update)
    {
        return JsonSerializer.Serialize(update);
    }
}