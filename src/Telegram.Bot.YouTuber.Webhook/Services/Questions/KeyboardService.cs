using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.YouTuber.Core.Extensions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Questions;

internal sealed class KeyboardService : IKeyboardService
{
    #region Implementation of IKeyboardService

    public InlineKeyboardMarkup GetQuestionKeyboard(QuestionContext context)
    {
        var buttons = context.Buttons.AsNotNull();

        List<IEnumerable<InlineKeyboardButton>> list = new List<IEnumerable<InlineKeyboardButton>>();

        int i = 0;
        while (i < buttons.Count)
        {
            IEnumerable<InlineKeyboardButton> line = Take2(ref i, buttons);
            list.Add(line);
        }
        
        return new InlineKeyboardMarkup(list);
    }

    #endregion

    private List<InlineKeyboardButton> Take2(ref int index, IReadOnlyList<QuestionButton> source)
    {
        List<InlineKeyboardButton> list = new(2);
        for (int i = 0; i < 2 && index < source.Count; i++)
        {
            var button = source[index];

            list.Add(new InlineKeyboardButton(button.Caption)
            {
                CallbackData = button.Data
            });

            index++;
        }

        return list;
    }
}