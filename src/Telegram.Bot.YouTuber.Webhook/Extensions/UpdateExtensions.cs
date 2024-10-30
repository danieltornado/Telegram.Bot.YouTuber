using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace Telegram.Bot.YouTuber.Webhook.Extensions;

public static class UpdateExtensions
{
    public static string CreateJson(this Update update)
    {
        return JsonConvert.SerializeObject(update);
    }
}