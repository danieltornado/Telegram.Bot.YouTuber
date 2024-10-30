using System.Diagnostics.CodeAnalysis;

namespace Telegram.Bot.YouTuber.Webhook.Services.Files;

public interface IFileService
{
    FileData CreateVideoFile(Guid sessionId);
    FileData CreateAudioFile(Guid sessionId);
    FileData CreateFinalFile(Guid sessionId, string extension);
    Stream? OpenFinalFile(Guid sessionId);
    Task DeleteSessionAsync(Guid sessionId, CancellationToken ct);
}