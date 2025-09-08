using VideoLibrary;

namespace Telegram.Bot.YouTuber.Webhook.BL.Implementations.Downloading;

internal sealed class CustomDelegatingClient : DelegatingClient
{
    private readonly IHttpClientFactory? _httpClientFactory;

    public CustomDelegatingClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    #region Overrides of DelegatingClient

    protected override HttpClient MakeClient(HttpMessageHandler handler)
    {
        // Delegating client has a call of the MakeClient
        return _httpClientFactory?.CreateClient("youtube_delegating_client")!;
    }

    protected override HttpMessageHandler MakeHandler()
    {
        return null!;
    }

    #endregion
}