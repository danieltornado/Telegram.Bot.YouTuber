using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.YouTuber.Core.Extensions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Questions;

internal sealed class KeyboardService : IKeyboardService
{
    #region Implementation of IKeyboardService

    public InlineKeyboardMarkup GetQuestionKeyboard(QuestionContext context)
    {
        var list = new List<InlineKeyboardButton>(context.Buttons.AsNotNull(paramName: nameof(context.Buttons)).Count);
        foreach (var button in context.Buttons!)
        {
            list.Add(new InlineKeyboardButton(button.Caption)
            {
                CallbackData = button.Data
            });
        }

        return new InlineKeyboardMarkup(list);
    }

    #endregion
}