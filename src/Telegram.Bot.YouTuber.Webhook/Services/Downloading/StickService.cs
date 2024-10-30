﻿using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FFMpegCore;
using Telegram.Bot.YouTuber.Core.Extensions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

internal sealed class StickService : IStickService
{
    private readonly string _tempPath;
    private readonly string _binaryPath;

    public StickService(IConfiguration configuration)
    {
        // https://stackoverflow.com/questions/43709657/how-to-get-root-directory-of-project-in-asp-net-core-directory-getcurrentdirect
        var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;

        _tempPath = Path.Combine(currentDirectory, "temp");
        _binaryPath = configuration.GetValue<string>("FFMpegPath").AsNotNull();
    }

    #region Implementation of IStickService

    public async Task StickAsync(string videoPath, string audioPath, string destinationPath, CancellationToken ct)
    {
        await FFMpegArguments
            .FromFileInput(videoPath)
            .AddFileInput(audioPath)
            .OutputToFile(destinationPath)
            .Configure(options =>
            {
                options.BinaryFolder = _binaryPath;
                options.TemporaryFilesFolder = GetFFMpegTempPath();
            })
            .ProcessAsynchronously();
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