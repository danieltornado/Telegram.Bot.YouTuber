using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;
using Telegram.Bot.YouTuber.Webhook.BL.Implementations;
using Xunit;

namespace Telegram.Bot.YouTuber.Webhook.Tests.BL;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class SessionMediaContextUIComparerTests
{
    [Fact]
    public void WhenMedia_Compare_ShouldBeValid()
    {
        // arrange
        SessionMediaContextUIComparer comparer = new();

        List<SessionMediaContext> list = new()
        {
            new()
            {
                IsSkipped = true
            },
            new()
            {
                Format = "WEBM",
                Quality = "720",
                ContentLength = 100
            },
            new()
            {
                Format = "MP4",
                Quality = "720",
                ContentLength = 1000
            },
            new()
            {
                Format = "MP4",
                Quality = "1080",
                ContentLength = 10000
            }
        };

        // act
        var result = list.OrderBy(e => e, comparer).ToList();

        // assert
        result.Should().HaveCount(4);
        result.Should().HaveElementAt(0, list[3]);
        result.Should().HaveElementAt(1, list[2]);
        result.Should().HaveElementAt(2, list[1]);
        result.Should().HaveElementAt(3, list[0]);
    }
}