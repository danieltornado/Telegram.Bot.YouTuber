using VideoLibrary;

namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Downloading;

/// <summary>
/// The instance overrides default behavior in <see cref="VideoLibrary.YouTube"/>: when the client gets a media uri the factory uses custom delegating client with a <see cref="IHttpClientFactory"/> inside
/// </summary>
public interface IDelegatingClientFactory
{
    DelegatingClient Create();
}