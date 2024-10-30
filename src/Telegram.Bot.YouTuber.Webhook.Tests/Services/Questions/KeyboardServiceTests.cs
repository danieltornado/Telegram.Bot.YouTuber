using FluentAssertions;
using Telegram.Bot.YouTuber.Webhook.Services.Questions;

namespace Telegram.Bot.YouTuber.Webhook.Tests.Services.Questions;

public sealed class KeyboardServiceTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 1)]
    [InlineData(4, 2)]
    [InlineData(5, 2)]
    [InlineData(6, 2)]
    [InlineData(7, 3)]
    [InlineData(8, 3)]
    [InlineData(9, 3)]
    public void WhenSomeButtons_GetQuestionKeyboard_ShouldCreateSomeLine(int countOfButtons, int countOfLines)
    {
        // Arrange
        QuestionContext context = new();
        
        List<QuestionButton> buttons = new();        
        for (int i = 0; i < countOfButtons; i++)
        {
            buttons.Add(new QuestionButton
            {
                Caption = i.ToString(),
                Data = i.ToString()
            });
        }
        
        context.Buttons = buttons;
        
        // Act
        var service = new KeyboardService();
        var keyboard = service.GetQuestionKeyboard(context);

        // Assert
        keyboard.InlineKeyboard.Should().HaveCount(countOfLines);
        keyboard.InlineKeyboard.SelectMany(e => e).Should().HaveCount(countOfButtons);
    }
}