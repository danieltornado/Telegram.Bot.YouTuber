using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Telegram.Bot.YouTuber.Webhook.Tests;

public sealed class XunitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _testOutputHelper;

    public XunitLoggerProvider(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    #region IDisposable

    /// <inheritdoc />
    public void Dispose()
    {
        // nothing
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName)
    {
        return new XunitLogger(categoryName, _testOutputHelper);
    }

    #endregion
}