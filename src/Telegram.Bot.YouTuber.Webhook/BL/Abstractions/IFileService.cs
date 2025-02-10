namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions;

public interface IFileService
{
    string GenerateVideoFilePath(Guid fileId);
    string GenerateAudioFilePath(Guid fileId);
    string GenerateFinalFilePath(Guid fileId, string extension);
    Stream? OpenFinalFile(Guid fileId);
    Task DeleteDownloadingAsync(Guid downloadingId, CancellationToken ct);
}