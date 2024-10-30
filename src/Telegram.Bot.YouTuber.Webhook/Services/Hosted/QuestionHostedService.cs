using Telegram.Bot.YouTuber.Core.Extensions;
using Telegram.Bot.YouTuber.Webhook.Extensions;
using Telegram.Bot.YouTuber.Webhook.Services.Downloading;
using Telegram.Bot.YouTuber.Webhook.Services.Questions;
using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Hosted;

public sealed class QuestionHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IQuestionQueueService _questionQueueService;
    private readonly IDownloadQueueService _downloadQueueService;
    private readonly ILogger<QuestionHostedService> _logger;

    public QuestionHostedService(
        IServiceProvider serviceProvider,
        IQuestionQueueService questionQueueService,
        IDownloadQueueService downloadQueueService,
        ILogger<QuestionHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _questionQueueService = questionQueueService;
        _downloadQueueService = downloadQueueService;
        _logger = logger;
    }

    #region Overrides of BackgroundService

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-8.0&tabs=visual-studio

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DoWorkAsync(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Question service cancelled");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Question service failed");

                if (!stoppingToken.IsCancellationRequested)
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    #endregion

    private async Task DoWorkAsync(CancellationToken ct)
    {
        var context = await _questionQueueService.DequeueAsync(ct);

        _logger.LogInformation("Started question task");

        using var scope = _serviceProvider.CreateScope();

        if (context.HasAudio())
        {
            // all is ready for downloading
            await SendDownloadTask(scope.ServiceProvider, context, ct);
        }
        else if (context.HasVideo())
        {
            // video is already selected
            // asking for an audio
            await SendAudioQuestionMessage(scope.ServiceProvider, context, ct);
        }
        else
        {
            // nothing is selected
            // asking for a video
            await SendVideoQuestionMessage(scope.ServiceProvider, context, ct);
        }

        _logger.LogInformation("Finished question task");
    }

    private async Task SendDownloadTask(IServiceProvider serviceProvider, SessionContext context, CancellationToken ct)
    {
        var sessionService = serviceProvider.GetRequiredService<ISessionService>();
        var telegramService = serviceProvider.GetRequiredService<ITelegramService>();

        // saving current state
        await sessionService.SaveSessionAsync(context, ct);

        if (context.IsSuccess)
        {
            await _downloadQueueService.QueueAsync(context, ct);
        }
        else
        {
            _logger.LogError(context.Error, $"Failed to save a session: {context.Id}");

            await sessionService.CompleteSessionAsync(context, ct);

            await telegramService.SendMessageAsync(context.ChatId, context.MessageId, "Internal server error", ct);
        }
    }

    private async Task SendAudioQuestionMessage(IServiceProvider serviceProvider, SessionContext context, CancellationToken ct)
    {
        var sessionService = serviceProvider.GetRequiredService<ISessionService>();
        var telegramService = serviceProvider.GetRequiredService<ITelegramService>();

        var questionService = serviceProvider.GetRequiredService<IQuestionService>();
        var questionContext = await questionService.GetAudioQuestionAsync(context, ct);
        if (!questionContext.IsSuccess)
        {
            _logger.LogError(questionContext.Error, "Failed to get an audio question");

            await sessionService.CompleteSessionAsync(context, ct);

            await telegramService.SendMessageAsync(context.ChatId, context.MessageId, "Internal server error", ct);
        }
        else
        {
            // The session got a video answer in the MessageController and got audio items

            // saving current state
            await sessionService.SaveSessionAsync(context, ct);

            if (context.IsSuccess)
            {
                var keyboardService = serviceProvider.GetRequiredService<IKeyboardService>();
                var keyboardMarkup = keyboardService.GetQuestionKeyboard(questionContext);

                await telegramService.SendKeyboardAsync(context.ChatId, context.MessageId, questionContext.Title.AsNotNull(message: "Title is null"), keyboardMarkup, ct);
            }
            else
            {
                _logger.LogError(context.Error, $"Failed to save a session: {context.Id}");

                await sessionService.CompleteSessionAsync(context, ct);

                await telegramService.SendMessageAsync(context.ChatId, context.MessageId, "Internal server error", ct);
            }
        }
    }

    private async Task SendVideoQuestionMessage(IServiceProvider serviceProvider, SessionContext context, CancellationToken ct)
    {
        var sessionService = serviceProvider.GetRequiredService<ISessionService>();
        var telegramService = serviceProvider.GetRequiredService<ITelegramService>();

        var questionService = serviceProvider.GetRequiredService<IQuestionService>();
        var questionContext = await questionService.GetVideoQuestionAsync(context, ct);
        if (!questionContext.IsSuccess)
        {
            _logger.LogError(questionContext.Error, "Failed to get a video question");

            await sessionService.CompleteSessionAsync(context, ct);

            await telegramService.SendMessageAsync(context.ChatId, context.MessageId, "Internal server error", ct);
        }
        else
        {
            // Session was not changed before this case.
            // At the moment the session has video items.

            // saving current state
            await sessionService.SaveSessionAsync(context, ct);

            if (context.IsSuccess)
            {
                var keyboardService = serviceProvider.GetRequiredService<IKeyboardService>();
                var keyboardMarkup = keyboardService.GetQuestionKeyboard(questionContext);

                await telegramService.SendKeyboardAsync(context.ChatId, context.MessageId, questionContext.Title.AsNotNull(message: "Title is null"), keyboardMarkup, ct);
            }
            else
            {
                _logger.LogError(context.Error, $"Failed to save a session: {context.Id}");

                await sessionService.CompleteSessionAsync(context, ct);

                await telegramService.SendMessageAsync(context.ChatId, context.MessageId, "Internal server error", ct);
            }
        }
    }
}