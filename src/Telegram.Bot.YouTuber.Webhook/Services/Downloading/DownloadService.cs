using Telegram.Bot.YouTuber.Webhook.Services.Files;
using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

internal sealed class DownloadService : IDownloadService
{
    private readonly YouTubeClient _youTubeClient;
    private readonly IFileService _fileService;
    private readonly IStickService _stickService;

    public DownloadService(YouTubeClient youTubeClient, IFileService fileService, IStickService stickService)
    {
        _youTubeClient = youTubeClient;
        _fileService = fileService;
        _stickService = stickService;
    }

    #region Implementation of IDownloadService

    public async Task DownloadAsync(SessionContext context, CancellationToken ct)
    {
        var video = context.Videos.FirstOrDefault(e => e.Id == context.VideoId);
        if (video is null)
        {
            context.IsSuccess = false;
            context.Error = new Exception($"Wrong video: {context.VideoId}");
            return;
        }

        var audio = context.Audios.FirstOrDefault(e => e.Id == context.AudioId);
        if (audio is null)
        {
            context.IsSuccess = false;
            context.Error = new Exception($"Wrong audio: {context.AudioId}");
            return;
        }

        try
        {
            // 1. download video
            await using var videoData = _fileService.CreateVideoFile(context.Id);
            await _youTubeClient.DownloadAsync(video.InternalUrl, videoData.Stream, ct);

            // 2. download audio
            await using var audioData = _fileService.CreateAudioFile(context.Id);
            await _youTubeClient.DownloadAsync(audio.InternalUrl, audioData.Stream, ct);

            // 3. ffmpeg
            await using var finalData = _fileService.CreateFinalFile(context.Id);
            await _stickService.StickAsync(videoData.FilePath, audioData.FilePath, finalData.FilePath, ct);
        }
        catch (Exception e)
        {
            context.IsSuccess = false;
            context.Error = e;
        }
    }

    #endregion
}