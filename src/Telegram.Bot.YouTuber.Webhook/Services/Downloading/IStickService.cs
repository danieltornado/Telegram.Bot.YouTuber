namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

public interface IStickService
{
    Task ConvertVideoAsync(string videoPath, string destinationPath, CancellationToken ct);
    Task ConvertAudioAsync(string audioPath, string destinationPath, CancellationToken ct);
    Task StickAsync(string videoPath, string audioPath, string destinationPath, CancellationToken ct);
}