using FluentAssertions;
using Telegram.Bot.YouTuber.Webhook.Services.Questions;

namespace Telegram.Bot.YouTuber.Webhook.Tests.Services.Questions;

public sealed class KeyboardServiceTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 2)]
    [InlineData(4, 2)]
    [InlineData(5, 3)]
    [InlineData(6, 3)]
    [InlineData(7, 4)]
    [InlineData(8, 4)]
    [InlineData(9, 5)]
    [InlineData(10, 5)]
    public void WhenSomeButtons_GetQuestionKeyboard_ShouldCreateSomeLine(int countOfButtons, int countOfLines)
    {
        // Arrange
        List<QuestionButton> buttons = new();
        for (int i = 0; i < countOfButtons; i++)
        {
            buttons.Add(new QuestionButton
            {
                Caption = i.ToString(),
                Data = i.ToString()
            });
        }

        // Act
        var service = new KeyboardService();
        var keyboard = service.GetQuestionKeyboard(buttons);

        // Assert
        keyboard.InlineKeyboard.Should().HaveCount(countOfLines);
        keyboard.InlineKeyboard.SelectMany(e => e).Should().HaveCount(countOfButtons);
    }
}