namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Media;

public interface IStickService
{
    Task ConvertVideoAsync(string videoPath, string destinationPath, CancellationToken ct);
    Task ConvertAudioAsync(string audioPath, string destinationPath, CancellationToken ct);
    Task StickAsync(string videoPath, string audioPath, string destinationPath, CancellationToken ct);
}