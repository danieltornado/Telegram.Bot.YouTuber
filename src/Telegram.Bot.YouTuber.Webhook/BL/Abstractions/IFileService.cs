using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions;

public interface IFileService
{
    string GenerateVideoFilePath(Guid fileId);
    string GenerateAudioFilePath(Guid fileId);
    string GenerateFinalFilePath(Guid fileId, string extension);
    Stream? OpenFinalFile(Guid fileId);
    Task DeleteDownloading(Guid downloadingId, CancellationToken ct);

    Task ThrowIfDoesNotHasAvailableFreeSpace(CancellationToken ct, SessionMediaContext media, params SessionMediaContext[] medias);
}