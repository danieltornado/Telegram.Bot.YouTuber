using System.Net.Http.Headers;
using AutoMapper;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Downloading;
using Telegram.Bot.YouTuber.Webhook.Extensions;
using VideoLibrary;
using VideoInfo = Telegram.Bot.YouTuber.Webhook.BL.Abstractions.VideoInfo;

namespace Telegram.Bot.YouTuber.Webhook.BL.Implementations.Downloading;

internal sealed class YouTubeClient : YouTube, IYouTubeClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDelegatingClientFactory _delegatingClientFactory;
    private readonly IMapper _mapper;

    public YouTubeClient(IHttpClientFactory httpClientFactory, IDelegatingClientFactory delegatingClientFactory, IMapper mapper)
    {
        _httpClientFactory = httpClientFactory;
        _delegatingClientFactory = delegatingClientFactory;
        _mapper = mapper;
    }

    #region Implementation of IYouTubeClient

    /// <inheritdoc />
    public async Task DownloadAsync(string internalUrl, long? contentLength, Stream destination, CancellationToken ct)
    {
        const long chunkSize = 10_485_760;

        var httpClient = MakeClient();

        if (!contentLength.HasValue)
        {
            contentLength = await GetContentLengthAsync(httpClient, internalUrl, ct);
        }

        if (!contentLength.HasValue || contentLength.Value < 1)
            throw new ArgumentException("Could not get content length", nameof(contentLength));

        long size = contentLength.Value;

        var segmentCount = (int)Math.Ceiling(1.0 * size / chunkSize);
        for (var i = 0; i < segmentCount; i++)
        {
            var from = i * chunkSize;
            var to = (i + 1) * chunkSize - 1;
            var request = new HttpRequestMessage(HttpMethod.Get, internalUrl);
            request.Headers.Range = new RangeHeaderValue(from, to);
            using (request)
            {
                // Download Stream
                var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
                if (response.IsSuccessStatusCode)
                    response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync(ct);

                //File Steam
                var buffer = new byte[81920];
                int bytesCopied;
                do
                {
                    bytesCopied = await stream.ReadAsync(buffer, 0, buffer.Length, ct);
                    await destination.WriteAsync(buffer, 0, bytesCopied, ct);
                } while (bytesCopied > 0);
            }
        }
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<VideoInfo> Video, IReadOnlyList<AudioInfo> Audio)> GetMetadataAsync(string url, CancellationToken ct)
    {
        var allVideos = await GetAllVideosAsync(url).AsReadOnlyListAsync();
        if (allVideos.Count > 0)
        {
            var resultVideo = new List<VideoInfo>(allVideos.Count);
            var resultAudio = new List<AudioInfo>(allVideos.Count);

            foreach (var youTubeVideo in allVideos)
            {
                if (youTubeVideo.AdaptiveKind == AdaptiveKind.Video)
                {
                    var videoInfo = await ToVideoInfo(youTubeVideo);
                    resultVideo.Add(videoInfo);
                }
                else if (youTubeVideo.AdaptiveKind == AdaptiveKind.Audio)
                {
                    var audioInfo = await ToAudioInfo(youTubeVideo);
                    resultAudio.Add(audioInfo);
                }
            }

            return (Video: resultVideo, Audio: resultAudio);
        }

        return (Video: Array.Empty<VideoInfo>(), Audio: Array.Empty<AudioInfo>());
    }

    #endregion

    private HttpClient MakeClient()
    {
        return _httpClientFactory.CreateClient();
    }

    private Task<string> GetInternalUrlAsync(YouTubeVideo video)
    {
        return video.GetUriAsync(_delegatingClientFactory.Create);
    }

    private async Task<long?> GetContentLengthAsync(HttpClient client, string internalUrl, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, internalUrl);
        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        return response.Content.Headers.ContentLength;
    }

    private async Task<VideoInfo> ToVideoInfo(YouTubeVideo video)
    {
        string internalUrl = await GetInternalUrlAsync(video);

        var info = _mapper.Map<VideoInfo>(video);
        info.InternalUrl = internalUrl;

        return info;
    }

    private async Task<AudioInfo> ToAudioInfo(YouTubeVideo audio)
    {
        string internalUrl = await GetInternalUrlAsync(audio);

        var info = _mapper.Map<AudioInfo>(audio);
        info.InternalUrl = internalUrl;

        return info;
    }
}