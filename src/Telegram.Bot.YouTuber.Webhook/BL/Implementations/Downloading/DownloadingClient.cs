using Telegram.Bot.YouTuber.Core.Extensions;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Downloading;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Media;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;
using Telegram.Bot.YouTuber.Webhook.BL.Implementations.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.BL.Implementations.Downloading;

internal sealed class DownloadingClient : IDownloadingClient
{
    private readonly IYouTubeClient _youTubeClient;
    private readonly IFileService _fileService;
    private readonly IStickService _stickService;
    private readonly IDownloadingService _downloadingService;
    private readonly ITelegramService _telegramService;

    public DownloadingClient(
        IYouTubeClient youTubeClient,
        IFileService fileService,
        IStickService stickService,
        IDownloadingService downloadingService,
        ITelegramService telegramService)
    {
        _youTubeClient = youTubeClient;
        _fileService = fileService;
        _stickService = stickService;
        _downloadingService = downloadingService;
        _telegramService = telegramService;
    }

    #region Implementation of IDownloadService

    /// <inheritdoc />
    public async Task<Guid> DownloadAsync(SessionContext session, SessionMediaContext video, SessionMediaContext audio, CancellationToken ct)
    {
        var downloadingId = await _downloadingService.StartDownloadingAsync(session.Id, video, audio, ct);

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

                // warning about combining webm+aac
                if (string.Equals(video.Format, "webm", StringComparison.InvariantCultureIgnoreCase) && string.Equals(audio.Format, "aac", StringComparison.InvariantCultureIgnoreCase))
                {
                    extension = ".mkv";

                    await _telegramService.SendWarningWebmAacAsync(session.ChatId, ct);

                    // 1a. set a new video extension
                    await _downloadingService.UpdateDownloadingVideoExtensionAsync(downloadingId, extension, ct);
                }

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