using FluentAssertions;
using Telegram.Bot.YouTuber.Webhook.Extensions;

namespace Telegram.Bot.YouTuber.Webhook.Tests.Extensions;

public sealed class BytesToHumanTests
{
    [Theory]
    [InlineData(0L, "0b")]
    [InlineData(1, "1.00b")]
    [InlineData(1024L - 1, "1023.00b")]
    [InlineData(1024L, "1.00Kb")]
    [InlineData(1024L + 1, "1.00Kb")]
    [InlineData(1024L + 512, "1.50Kb")]
    [InlineData(1024L * 1024 - 1, "1024.00Kb")]
    [InlineData(1024L * 1024, "1.00Mb")]
    [InlineData(1024L * 1024 + 1, "1.00Mb")]
    [InlineData(1024L * 1024 + 512 * 1024, "1.50Mb")]
    [InlineData(1024L * 1024 * 1024 - 1, "1024.00Mb")]
    [InlineData(1024L * 1024 * 1024, "1.00Gb")]
    [InlineData(1024L * 1024 * 1024 + 1, "1.00Gb")]
    [InlineData(1024L * 1024 * 1024 + 512 * 1024 * 1024, "1.50Gb")]
    [InlineData(1024L * 1024 * 1024 * 1024 - 1, "1024.00Gb")]
    [InlineData(1024L * 1024 * 1024 * 1024, "1.00Tb")]
    public void WhenSize_BytesToHuman_ShouldBeExpected(long bytes, string expected)
    {
        // arrange
        // act
        // assert
        bytes.BytesToHuman().Should().Be(expected);
    }
}