using VideoLibrary;

namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

public interface IDelegatingClientFactory
{
    DelegatingClient Create();
}