﻿using VideoLibrary;

namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

internal sealed class DelegatingClientFactory : IDelegatingClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DelegatingClientFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    #region Implementation of IDelegatingClientFactory

    public DelegatingClient Create()
    {
        return new CustomDelegatingClient(_httpClientFactory);
    }

    #endregion
}