using Telegram.Bot.Types;
using Telegram.Bot.YouTuber.Webhook.Extensions;
using Telegram.Bot.YouTuber.Webhook.Services.Questions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Messaging;

internal sealed class MessageService : IMessageService
{
    private readonly ILogger<MessageService> _logger;

    public MessageService(ILogger<MessageService> logger)
    {
        _logger = logger;
    }

    #region Implementation of IMessageService

    public Task<CallbackQueryContext> ParseCallbackQueryAsync(Update update, CancellationToken ct)
    {
        CallbackQueryContext context = new();
        context.IsSuccess = true;
        context.Json = update.CreateJson();

        var callbackQuery = update.CallbackQuery;
        if (callbackQuery is null)
        {
            context.IsSuccess = false;
            context.Error = new Exception("Invalid CallbackQuery");
            return context.AsTask();
        }

        string? data = callbackQuery.Data;

        if (data is null)
        {
            context.IsSuccess = false;
            context.Error = new Exception("Data is null");
            return context.AsTask();
        }

        try
        {
            var questionData = ParseData(data);

            context.Type = questionData.Type;
            context.SessionId = questionData.SessionId;
            context.Num = questionData.Num;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error parsing {Data}", data);

            context.IsSuccess = false;
            context.Error = e;
            return context.AsTask();
        }

        return context.AsTask();
    }

    #endregion

    private QuestionData ParseData(string data)
    {
        return QuestionData.FromCallbackQueryData(data);
    }
}