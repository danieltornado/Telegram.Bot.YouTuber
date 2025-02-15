using AutoMapper;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.YouTuber.Core.Extensions;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Downloading;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Questions;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Queues;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;
using Telegram.Bot.YouTuber.Webhook.BL.Implementations.Sessions;
using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;

namespace Telegram.Bot.YouTuber.Webhook.BL.Implementations;

internal sealed class MessageHandling : IMessageHandling
{
    private readonly ISessionService _sessionService;
    private readonly ITelegramService _telegramService;
    private readonly IYouTubeClient _youTubeClient;
    private readonly IKeyboardService _keyboardService;
    private readonly ISessionsQueueService _sessionQueueService;
    private readonly IMapper _mapper;
    private readonly ILogger<MessageHandling> _logger;

    public MessageHandling(
        ISessionService sessionService,
        ITelegramService telegramService,
        IYouTubeClient youTubeClient,
        IKeyboardService keyboardService,
        ISessionsQueueService sessionQueueService,
        IMapper mapper,
        ILogger<MessageHandling> logger)
    {
        _sessionService = sessionService;
        _telegramService = telegramService;
        _youTubeClient = youTubeClient;
        _keyboardService = keyboardService;
        _sessionQueueService = sessionQueueService;
        _mapper = mapper;
        _logger = logger;
    }

    #region Implementation of IMessageHandling

    public async Task HandleMessageAsync(Update update, RequestContext requestContext, CancellationToken ct)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                await StartNewSession(update, ct);
                break;
            case UpdateType.CallbackQuery:
                await ContinueSession(update, requestContext, ct);
                break;
        }
    }

    #endregion

    private async Task StartNewSession(Update update, CancellationToken ct)
    {
        var startSessionContext = _mapper.Map<StartSessionContext>(update);
        if (IsStartCommand(startSessionContext.Url))
        {
            await _telegramService.SendWelcomeMessageAsync(startSessionContext.ChatId, ct);
            return;
        }

        if (IsValidYouTubeUrl(startSessionContext.Url) is false)
        {
            await _telegramService.SendInvalidUrlMessageAsync(startSessionContext.ChatId, startSessionContext.MessageId, ct);
            return;
        }

        SessionContext sessionContext;
        try
        {
            sessionContext = await _sessionService.StartSessionAsync(startSessionContext, ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to start new session");

            await _telegramService.SendInternalServerErrorAsync(startSessionContext.ChatId, startSessionContext.MessageId, e, ct);

            return;
        }

        await _telegramService.SendMessageAsync(sessionContext.ChatId, sessionContext.MessageId, "Receiving metadata...", ct);

        List<SessionMediaContext> metadata;
        try
        {
            metadata = await PrepareYouTubeMetadata(sessionContext, ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to prepare youtube metadata");

            await _sessionService.SetFailedSessionAsync(sessionContext.Id, e.ToString(), ct);

            await _telegramService.SendInternalServerErrorAsync(startSessionContext.ChatId, startSessionContext.MessageId, e, ct);

            return;
        }

        await SendVideoQuestion(sessionContext, metadata.Where(e => e.Type == MediaType.Video), ct);
    }

    private async Task<List<SessionMediaContext>> PrepareYouTubeMetadata(SessionContext sessionContext, CancellationToken ct)
    {
        var (video, audio) = await _youTubeClient.GetMetadataAsync(sessionContext.Url.AsNotNull(message: "Url is required"), ct);
        var media = await _sessionService.SaveMediaAsync(sessionContext.Id, video, audio, ct);

        return media;
    }

    private async Task ContinueSession(Update update, RequestContext requestContext, CancellationToken ct)
    {
        var continueSessionContext = _mapper.Map<ContinueSessionContext>(update);

        SessionContext sessionContext;
        try
        {
            // get data
            var questionData = QuestionData.FromCallbackQueryData(update.CallbackQuery.AsNotNull(message: "CallbackQuery is required").Data.AsNotNull("CallbackQuery.Data is required"));

            // save an answer
            await _sessionService.SaveSessionAnswerAsync(questionData.MediaId, continueSessionContext.Json, ct);

            // check a session
            sessionContext = await _sessionService.GetSessionByMediaAsync(questionData.MediaId, ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to continue session");

            await _telegramService.SendInternalServerErrorAsync(continueSessionContext.ChatId, null, e, ct);

            return;
        }

        if (sessionContext.HasAudio() && sessionContext.HasVideo())
        {
            // ready for download
            sessionContext.RequestContext = requestContext;

            await _sessionQueueService.QueueAsync(sessionContext, ct);
        }
        else if (sessionContext.HasVideo())
        {
            await AskForAudio(sessionContext, ct);
        }
        else if (sessionContext.HasAudio())
        {
            await AskForVideo(sessionContext, ct);
        }
        else
        {
            _logger.LogWarning("Undetectable application state");

            await _telegramService.SendInternalServerErrorAsync(continueSessionContext.ChatId, continueSessionContext.MessageId, null, ct);
        }
    }

    private async Task AskForVideo(SessionContext sessionContext, CancellationToken ct)
    {
        List<SessionMediaContext> media;
        try
        {
            media = await _sessionService.GetMediaBySessionAsync(sessionContext.Id, MediaType.Video, ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to ask for video");

            await _sessionService.SetFailedSessionAsync(sessionContext.Id, e.ToString(), ct);

            await _telegramService.SendInternalServerErrorAsync(sessionContext.ChatId, null, e, ct);

            return;
        }

        await SendVideoQuestion(sessionContext, media, ct);
    }

    private async Task AskForAudio(SessionContext sessionContext, CancellationToken ct)
    {
        List<SessionMediaContext> media;
        try
        {
            media = await _sessionService.GetMediaBySessionAsync(sessionContext.Id, MediaType.Audio, ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to ask for video");

            await _sessionService.SetFailedSessionAsync(sessionContext.Id, e.ToString(), ct);

            await _telegramService.SendInternalServerErrorAsync(sessionContext.ChatId, null, e, ct);

            return;
        }

        await SendAudioQuestion(sessionContext, media, ct);
    }

    private async Task SendVideoQuestion(SessionContext sessionContext, IEnumerable<SessionMediaContext> videos, CancellationToken ct)
    {
        var buttons = _mapper.Map<List<QuestionButton>>(videos.OrderBy(e => e, new SessionMediaContextUIComparer()));

        var keyboard = _keyboardService.GetQuestionKeyboard(buttons);
        await _telegramService.SendKeyboardAsync(sessionContext.ChatId, sessionContext.MessageId, "Video", keyboard, ct);
    }

    private async Task SendAudioQuestion(SessionContext sessionContext, IEnumerable<SessionMediaContext> audios, CancellationToken ct)
    {
        var buttons = _mapper.Map<List<QuestionButton>>(audios.OrderBy(e => e, new SessionMediaContextUIComparer()));

        var keyboard = _keyboardService.GetQuestionKeyboard(buttons);
        await _telegramService.SendKeyboardAsync(sessionContext.ChatId, sessionContext.MessageId, "Audio", keyboard, ct);
    }

    private bool IsStartCommand(string? url)
    {
        return string.Equals(url, "/start", StringComparison.InvariantCulture);
    }

    private bool IsValidYouTubeUrl(string? url)
    {
        // Valid cases
        // https://www.youtube.com/watch?v=
        // https://youtu.be/

        if (url is null)
            return false;

        if (url.StartsWith("https://www.youtube.com", StringComparison.InvariantCulture))
            return true;

        if (url.StartsWith("https://youtu.be", StringComparison.InvariantCulture))
            return true;

        return false;
    }
}