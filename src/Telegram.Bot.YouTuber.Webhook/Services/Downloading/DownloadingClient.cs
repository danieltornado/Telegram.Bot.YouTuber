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

    public DownloadingClient(
        YouTubeClient youTubeClient,
        IFileService fileService,
        IStickService stickService,
        IDownloadingService downloadingService)
    {
        _youTubeClient = youTubeClient;
        _fileService = fileService;
        _stickService = stickService;
        _downloadingService = downloadingService;
    }

    #region Implementation of IDownloadService

    public async Task<Guid> DownloadAsync(Guid sessionId, SessionMediaContext video, SessionMediaContext audio, CancellationToken ct)
    {
        var downloadingId = await _downloadingService.StartDownloadingAsync(sessionId, video, audio, ct);

        try
        {
            if (video.IsSkipped && audio.IsSkipped)
            {
                // nothing to download
            }
            else if (video.IsSkipped)
            {
                var extension = audio.GetExtension();

                // 1. download audio
                string audioPath = _fileService.GenerateAudioFilePath(downloadingId);
                await using (var audioStream = new FileStream(audioPath, FileMode.Create))
                {
                    await _youTubeClient.DownloadAsync(audio.InternalUrl.AsNotNull(message: "Audio url"), audio.ContentLength, audioStream, ct);
                }

                // 2. ffmpeg
                string finalPath = _fileService.GenerateFinalFilePath(downloadingId, extension);
                await _stickService.ConvertAudioAsync(audioPath, finalPath, ct);
            }
            else if (audio.IsSkipped)
            {
                var extension = video.GetExtension();

                // 1. download video
                string videoPath = _fileService.GenerateVideoFilePath(downloadingId);
                await using (var videoStream = new FileStream(videoPath, FileMode.Create))
                {
                    await _youTubeClient.DownloadAsync(video.InternalUrl.AsNotNull(message: "Video url"), video.ContentLength, videoStream, ct);
                }

                // 2. ffmpeg
                string finalPath = _fileService.GenerateFinalFilePath(downloadingId, extension);
                await _stickService.ConvertVideoAsync(videoPath, finalPath, ct);
            }
            else
            {
                var extension = video.GetExtension();

                // 1. download video
                string videoPath = _fileService.GenerateVideoFilePath(downloadingId);
                await using (var videoStream = new FileStream(videoPath, FileMode.Create))
                {
                    await _youTubeClient.DownloadAsync(video.InternalUrl.AsNotNull(message: "Video url"), video.ContentLength, videoStream, ct);
                }

                // 2. download audio
                string audioPath = _fileService.GenerateAudioFilePath(downloadingId);
                await using (var audioStream = new FileStream(audioPath, FileMode.Create))
                {
                    await _youTubeClient.DownloadAsync(audio.InternalUrl.AsNotNull(message: "Audio url"), audio.ContentLength, audioStream, ct);
                }

                // 3. ffmpeg
                string finalPath = _fileService.GenerateFinalFilePath(downloadingId, extension);
                await _stickService.StickAsync(videoPath, audioPath, finalPath, ct);
            }
        }
        catch (Exception e)
        {
            await _downloadingService.SetFailedDownloadingAsync(downloadingId, e.ToString(), ct);
            throw;
        }

        await _downloadingService.CompleteDownloadingAsync(downloadingId, ct);

        return downloadingId;
    }

    #endregion
}