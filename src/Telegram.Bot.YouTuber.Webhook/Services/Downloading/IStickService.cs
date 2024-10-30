namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

public interface IStickService
{
    Task StickAsync(string videoPath, string audioPath, string destinationPath, CancellationToken ct);
}