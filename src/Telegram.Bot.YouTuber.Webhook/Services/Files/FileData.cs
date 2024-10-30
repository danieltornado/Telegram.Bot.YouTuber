namespace Telegram.Bot.YouTuber.Webhook.Services.Files;

public sealed class FileData : IDisposable, IAsyncDisposable
{
    public required Stream Stream { get; set; }
    public required string FilePath { get; set; }

    #region Implementation of IDisposable

    public void Dispose()
    {
        Stream.Dispose();
    }

    #endregion

    #region Implementation of IAsyncDisposable

    public async ValueTask DisposeAsync()
    {
        await Stream.DisposeAsync();
    }

    #endregion
}