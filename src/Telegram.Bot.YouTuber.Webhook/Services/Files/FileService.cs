﻿using System.Reflection;

namespace Telegram.Bot.YouTuber.Webhook.Services.Files;

public sealed class FileService : IFileService
{
    private readonly string _filesDirectory;

    public FileService()
    {
        // https://stackoverflow.com/questions/43709657/how-to-get-root-directory-of-project-in-asp-net-core-directory-getcurrentdirect
        _filesDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "files");
    }

    #region Implementation of IFileService

    public FileData CreateVideoFile(Guid sessionId)
    {
        CheckFilesDirectory();
        string directory = GenerateSessionDirectory(sessionId);
        string filePath = Path.Combine(directory, "video.tmp");
        return new FileData
        {
            Stream = new FileStream(filePath, FileMode.Create),
            FilePath = filePath
        };
    }

    public FileData CreateAudioFile(Guid sessionId)
    {
        CheckFilesDirectory();
        string directory = GenerateSessionDirectory(sessionId);
        string filePath = Path.Combine(directory, "audio.tmp");
        return new FileData
        {
            Stream = new FileStream(filePath, FileMode.Create),
            FilePath = filePath
        };
    }

    public FileData CreateFinalFile(Guid sessionId)
    {
        CheckFilesDirectory();
        string directory = GenerateSessionDirectory(sessionId);
        string filePath = Path.Combine(directory, "final.tmp");
        return new FileData
        {
            Stream = new FileStream(filePath, FileMode.Create),
            FilePath = filePath
        };
    }

    public Stream? OpenFinalFile(Guid sessionId)
    {
        string directory = GetSessionDirectory(sessionId);
        string filePath = Path.Combine(directory, "final.tmp");
        if (File.Exists(filePath))
            return new FileStream(filePath, FileMode.Open);

        return null;
    }

    public Task DeleteSessionAsync(Guid sessionId, CancellationToken ct)
    {
        string directory = GetSessionDirectory(sessionId);
        if (!Directory.Exists(directory))
            return Task.CompletedTask;

        var files = Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly);
        foreach (var s in files)
        {
            File.Delete(s);
        }

        Directory.Delete(directory, false);
        return Task.CompletedTask;
    }

    #endregion

    private void CheckFilesDirectory()
    {
        if (!Directory.Exists(_filesDirectory))
            Directory.CreateDirectory(_filesDirectory);
    }
    
    private string GenerateSessionDirectory(Guid id)
    {
        string directory = Path.Combine(_filesDirectory, id.ToString("D"));
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        return directory;
    }

    private string GetSessionDirectory(Guid id)
    {
        return Path.Combine(_filesDirectory, id.ToString("D"));
    }
}