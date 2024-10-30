using System.Net.Http.Headers;
using Telegram.Bot.YouTuber.Webhook.Extensions;
using VideoLibrary;

namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

public sealed class YouTubeClient : YouTube
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDelegatingClientFactory _delegatingClientFactory;

    public YouTubeClient(IHttpClientFactory httpClientFactory, IDelegatingClientFactory delegatingClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _delegatingClientFactory = delegatingClientFactory;
    }

    public async Task DownloadAsync(string internalUrl, Stream destination, CancellationToken ct)
    {
        const long chunkSize = 10_485_760;

        var httpClient = MakeClient();

        long size = await GetContentLengthAsync(httpClient, internalUrl, ct) ?? 0;
        if (size < 1)
            throw new ArgumentException("Wrong size value", nameof(size));

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

    public async Task<IReadOnlyList<VideoInfo>> GetVideosAsync(string url, CancellationToken ct)
    {
        var allVideos = await GetAllVideosAsync(url).AsReadOnlyListAsync();
        if (allVideos.Count > 0)
        {
            var result = new List<VideoInfo>(allVideos.Count);

            foreach (var youTubeVideo in allVideos)
            {
                if (youTubeVideo.AdaptiveKind == AdaptiveKind.Video)
                {
                    var videoInfo = await ToVideoInfo(youTubeVideo);
                    result.Add(videoInfo);
                }
            }

            return result;
        }

        return Array.Empty<VideoInfo>();
    }

    public async Task<IReadOnlyList<AudioInfo>> GetAudiosAsync(string url, CancellationToken ct)
    {
        var allVideos = await GetAllVideosAsync(url).AsReadOnlyListAsync();
        if (allVideos.Count > 0)
        {
            var result = new List<AudioInfo>(allVideos.Count);

            foreach (var youTubeVideo in allVideos)
            {
                if (youTubeVideo.AdaptiveKind == AdaptiveKind.Audio)
                {
                    var audioInfo = await ToAudioInfo(youTubeVideo);
                    result.Add(audioInfo);
                }
            }

            return result;
        }

        return Array.Empty<AudioInfo>();
    }

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

        return new VideoInfo
        {
            Format = video.Format.ToString(),
            Quality = video.Resolution.ToString(),
            Title = video.Title,
            InternalUrl = internalUrl,
            FileExtension = video.FileExtension
        };
    }

    private async Task<AudioInfo> ToAudioInfo(YouTubeVideo audio)
    {
        string internalUrl = await GetInternalUrlAsync(audio);

        return new AudioInfo
        {
            Format = audio.AudioFormat.ToString(),
            Quality = audio.AudioBitrate.ToString(),
            Title = audio.Title,
            InternalUrl = internalUrl,
            FileExtension = audio.FileExtension
        };
    }
}