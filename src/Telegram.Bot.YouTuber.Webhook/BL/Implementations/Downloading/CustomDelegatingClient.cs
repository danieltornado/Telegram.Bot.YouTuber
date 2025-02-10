using VideoLibrary;

namespace Telegram.Bot.YouTuber.Webhook.BL.Implementations.Downloading;

internal sealed class CustomDelegatingClient : DelegatingClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CustomDelegatingClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    #region Overrides of DelegatingClient

    protected override HttpClient MakeClient(HttpMessageHandler handler)
    {
        return _httpClientFactory.CreateClient("youtube_delegating_client");
    }

    protected override HttpMessageHandler MakeHandler()
    {
        return null!;
    }

    #endregion
}