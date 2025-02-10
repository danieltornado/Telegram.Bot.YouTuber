using System.Reflection;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions;

namespace Telegram.Bot.YouTuber.Webhook.BL.Implementations;

public sealed class FileService : IFileService
{
    private readonly string _filesDirectory;

    public FileService()
    {
        // https://stackoverflow.com/questions/43709657/how-to-get-root-directory-of-project-in-asp-net-core-directory-getcurrentdirect
        _filesDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "files");
    }

    #region Implementation of IFileService

    public string GenerateVideoFilePath(Guid fileId)
    {
        CheckFilesDirectory();
        string directory = GenerateSessionDirectory(fileId);
        string filePath = Path.Combine(directory, "video.tmp");
        return filePath;
    }

    public string GenerateAudioFilePath(Guid fileId)
    {
        CheckFilesDirectory();
        string directory = GenerateSessionDirectory(fileId);
        string filePath = Path.Combine(directory, "audio.tmp");
        return filePath;
    }

    public string GenerateFinalFilePath(Guid fileId, string extension)
    {
        // ffmpeg can ask for an extension of the output file

        CheckFilesDirectory();
        string directory = GenerateSessionDirectory(fileId);

        if (extension.StartsWith("."))
            extension = extension.Substring(1);

        string filePath = Path.Combine(directory, $"final.{extension}");
        return filePath;
    }

    public Stream? OpenFinalFile(Guid fileId)
    {
        string directory = GetSessionDirectory(fileId);
        // must be single
        var filePath = Directory.EnumerateFiles(directory, "final.*").FirstOrDefault();
        if (File.Exists(filePath))
            return new FileStream(filePath, FileMode.Open);

        return null;
    }

    public Task DeleteDownloadingAsync(Guid downloadingId, CancellationToken ct)
    {
        string directory = GetSessionDirectory(downloadingId);
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