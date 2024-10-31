using Telegram.Bot.YouTuber.Core.Extensions;
using Telegram.Bot.YouTuber.Webhook.Extensions;
using Telegram.Bot.YouTuber.Webhook.Services.Files;
using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

internal sealed class DownloadingClient : IDownloadingClient
{
    private readonly YouTubeClient _youTubeClient;
    private readonly IFileService _fileService;
    private readonly IStickService _stickService;
    private readonly IDownloadingService _downloadingService;

    public DownloadingClient(YouTubeClient youTubeClient, IFileService fileService, IStickService stickService, IDownloadingService downloadingService)
    {
        _youTubeClient = youTubeClient;
        _fileService = fileService;
        _stickService = stickService;
        _downloadingService = downloadingService;
    }

    #region Implementation of IDownloadService

    public async Task<DownloadingContext> DownloadAsync(SessionContext sessionContext, CancellationToken ct)
    {
        DownloadingContext downloadingContext = new();
        downloadingContext.IsSuccess = true;

        var video = sessionContext.Videos.FirstOrDefault(e => e.Id == sessionContext.VideoId);
        if (video is null)
        {
            downloadingContext.IsSuccess = false;
            downloadingContext.Error = new Exception($"Wrong video: {sessionContext.VideoId}");
            return downloadingContext;
        }

        var audio = sessionContext.Audios.FirstOrDefault(e => e.Id == sessionContext.AudioId);
        if (audio is null)
        {
            downloadingContext.IsSuccess = false;
            downloadingContext.Error = new Exception($"Wrong audio: {sessionContext.AudioId}");
            return downloadingContext;
        }

        try
        {
            var downloadingId = await _downloadingService.StartDownloadingAsync(sessionContext, video, audio, ct);
            downloadingContext.Id = downloadingId;

            if (video.IsSkipped && audio.IsSkipped)
            {
                downloadingContext.IsSkipped = true;
            }
            else if (video.IsSkipped)
            {
                downloadingContext.Title = audio.GetTitle();
                downloadingContext.Extension = audio.GetExtension();

                downloadingContext.AudioQuality = audio.GetQuality();
                downloadingContext.AudioFormat = audio.GetFormat();

                // 1. download audio
                string audioPath = _fileService.GenerateAudioFilePath(downloadingId);
                await using (var audioStream = new FileStream(audioPath, FileMode.Create))
                {
                    await _youTubeClient.DownloadAsync(audio.InternalUrl.AsNotNull(message: "Audio url"), audioStream, ct);
                }

                // 2. ffmpeg
                string finalPath = _fileService.GenerateFinalFilePath(downloadingId, downloadingContext.Extension);
                await _stickService.ConvertAudioAsync(audioPath, finalPath, ct);
            }
            else if (audio.IsSkipped)
            {
                downloadingContext.Title = video.GetTitle();
                downloadingContext.Extension = video.GetExtension();

                downloadingContext.VideoQuality = video.GetQuality();
                downloadingContext.VideoFormat = video.GetFormat();

                // 1. download video
                string videoPath = _fileService.GenerateVideoFilePath(downloadingId);
                await using (var videoStream = new FileStream(videoPath, FileMode.Create))
                {
                    await _youTubeClient.DownloadAsync(video.InternalUrl.AsNotNull(message: "Video url"), videoStream, ct);
                }

                // 2. ffmpeg
                string finalPath = _fileService.GenerateFinalFilePath(downloadingId, downloadingContext.Extension);
                await _stickService.ConvertVideoAsync(videoPath, finalPath, ct);
            }
            else
            {
                downloadingContext.Title = video.GetTitle();
                downloadingContext.Extension = video.GetExtension();

                downloadingContext.VideoQuality = video.GetQuality();
                downloadingContext.VideoFormat = video.GetFormat();

                downloadingContext.AudioQuality = audio.GetQuality();
                downloadingContext.AudioFormat = audio.GetFormat();

                // 1. download video
                string videoPath = _fileService.GenerateVideoFilePath(downloadingId);
                await using (var videoStream = new FileStream(videoPath, FileMode.Create))
                {
                    await _youTubeClient.DownloadAsync(video.InternalUrl.AsNotNull(message: "Video url"), videoStream, ct);
                }

                // 2. download audio
                string audioPath = _fileService.GenerateAudioFilePath(downloadingId);
                await using (var audioStream = new FileStream(audioPath, FileMode.Create))
                {
                    await _youTubeClient.DownloadAsync(audio.InternalUrl.AsNotNull(message: "Audio url"), audioStream, ct);
                }

                // 3. ffmpeg
                string finalPath = _fileService.GenerateFinalFilePath(downloadingId, downloadingContext.Extension);
                await _stickService.StickAsync(videoPath, audioPath, finalPath, ct);
            }
        }
        catch (Exception e)
        {
            downloadingContext.IsSuccess = false;
            downloadingContext.Error = e;
        }

        await _downloadingService.CompleteDownloadingAsync(downloadingContext, ct);
        return downloadingContext;
    }

    #endregion
}