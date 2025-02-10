using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Telegram.Bot.YouTuber.Webhook.Tests;

public sealed class XunitLogger : ILogger
{
    private readonly string _categoryName;
    private readonly ITestOutputHelper _testOutputHelper;

    public XunitLogger(string categoryName, ITestOutputHelper testOutputHelper)
    {
        _categoryName = categoryName;
        _testOutputHelper = testOutputHelper;
    }
    
    #region Implementation of ILogger

    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => false,
            _ => true
        };
    }

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _testOutputHelper.WriteLine($"{_categoryName}|{logLevel.ToString()}|{formatter(state, exception)}");
    }

    #endregion
}