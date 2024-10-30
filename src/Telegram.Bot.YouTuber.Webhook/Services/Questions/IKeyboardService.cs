using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.YouTuber.Webhook.Services.Questions;

public interface IKeyboardService
{
    InlineKeyboardMarkup GetQuestionKeyboard(QuestionContext context);
}