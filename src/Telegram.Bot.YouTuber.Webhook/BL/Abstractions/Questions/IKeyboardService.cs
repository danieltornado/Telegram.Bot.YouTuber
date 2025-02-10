using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Questions;

public interface IKeyboardService
{
    InlineKeyboardMarkup GetQuestionKeyboard(IReadOnlyList<QuestionButton> buttons);
}