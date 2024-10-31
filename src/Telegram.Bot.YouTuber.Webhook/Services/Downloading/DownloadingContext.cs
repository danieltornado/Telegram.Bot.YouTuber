namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

public sealed class DownloadingContext : IContext
{
    public Guid Id { get; set; }
    public string? VideoTitle { get; set; }
    public string? VideoQuality { get; set; }
    public string? VideoFormat { get; set; }
    public string? VideoExtension { get; set; }
    public string? AudioTitle { get; set; }
    public string? AudioQuality { get; set; }
    public string? AudioFormat { get; set; }
    public string? AudioExtension { get; set; }
    public bool IsSkipped { get; set; }

    #region Implementation of IContext

    public bool IsSuccess { get; set; }
    public Exception? Error { get; set; }

    #endregion
}