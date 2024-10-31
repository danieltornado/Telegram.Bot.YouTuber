using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FFMpegCore;
using FFMpegCore.Enums;
using Telegram.Bot.YouTuber.Core.Extensions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

internal sealed class StickService : IStickService
{
    private readonly ILogger<StickService> _logger;
    private readonly string _tempPath;
    private readonly string _binaryPath;

    public StickService(IConfiguration configuration, ILogger<StickService> logger)
    {
        _logger = logger;

        // https://stackoverflow.com/questions/43709657/how-to-get-root-directory-of-project-in-asp-net-core-directory-getcurrentdirect
        var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;

        _tempPath = Path.Combine(currentDirectory, "temp");
        _binaryPath = configuration.GetValue<string>("FFMpegPath").AsNotNull();
    }

    #region Implementation of IStickService

    public async Task ConvertVideoAsync(string videoPath, string destinationPath, CancellationToken ct)
    {
        await using var destinationStream = new FileStream(destinationPath, FileMode.Create);
        await using var videoStream = new FileStream(videoPath, FileMode.Open);
        await videoStream.CopyToAsync(destinationStream, ct);
    }

    public async Task ConvertAudioAsync(string audioPath, string destinationPath, CancellationToken ct)
    {
        await using var destinationStream = new FileStream(destinationPath, FileMode.Create);
        await using var audioStream = new FileStream(audioPath, FileMode.Open);
        await audioStream.CopyToAsync(destinationStream, ct);
    }

    public async Task StickAsync(string videoPath, string audioPath, string destinationPath, CancellationToken ct)
    {
        _logger.LogInformation("Started merging video");
        
        await FFMpegArguments
            .FromFileInput(videoPath)
            .AddFileInput(audioPath)
            .OutputToFile(destinationPath, true,
                options =>
                {
                    // https://superuser.com/questions/277642/how-to-merge-audio-and-video-file-in-ffmpeg
                    // Copying the audio without re-encoding
                    options.WithCustomArgument("-c copy");
                })
            .Configure(options =>
            {
                options.BinaryFolder = _binaryPath;
                options.TemporaryFilesFolder = GetFFMpegTempPath();
                options.LogLevel = FFMpegLogLevel.Quiet;
            })
            .CancellableThrough(ct)
            .ProcessAsynchronously();
        
        _logger.LogInformation("Finished merging video");
    }

    #endregion

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private string GetFFMpegTempPath()
    {
        if (!Directory.Exists(_tempPath))
            Directory.CreateDirectory(_tempPath);

        return _tempPath;
    }
}