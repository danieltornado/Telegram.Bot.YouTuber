using AutoMapper;
using Telegram.Bot.YouTuber.Webhook.Extensions;
using Telegram.Bot.YouTuber.Webhook.Services.Downloading;
using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Questions;

internal sealed class QuestionService : IQuestionService
{
    private readonly YouTubeClient _youTubeClient;
    private readonly IMapper _mapper;

    public QuestionService(YouTubeClient youTubeClient, IMapper mapper)
    {
        _youTubeClient = youTubeClient;
        _mapper = mapper;
    }
    
    #region Implementation of IQuestionService

    public async Task<QuestionContext> GetVideoQuestionAsync(SessionContext sessionContext, CancellationToken ct)
    {
        QuestionContext questionContext = new();
        questionContext.IsSuccess = true;

        if (sessionContext.Url is null)
        {
            questionContext.IsSuccess = false;
            questionContext.Error = new Exception("URL is required");
            return questionContext;
        }

        IReadOnlyList<VideoInfo> list;
        try
        {
            list = await _youTubeClient.GetVideosAsync(sessionContext.Url, ct);
        }
        catch (Exception e)
        {
            questionContext.IsSuccess = false;
            questionContext.Error = e;
            return questionContext;
        }

        if (list.Count == 0)
        {
            questionContext.IsSuccess = false;
            questionContext.Error = new Exception("Could get videos from youtube");
            return questionContext;
        }
        
        sessionContext.SetVideoItems(_mapper, list);
        
        questionContext.Buttons = sessionContext.GetVideoButtons();
        
        questionContext.Title = "Video";
        
        return questionContext;
    }

    public async Task<QuestionContext> GetAudioQuestionAsync(SessionContext sessionContext, CancellationToken ct)
    {
        QuestionContext questionContext = new();
        questionContext.IsSuccess = true;

        if (sessionContext.Url is null)
        {
            questionContext.IsSuccess = false;
            questionContext.Error = new Exception("URL is required");
            return questionContext;
        }

        IReadOnlyList<AudioInfo> list;
        try
        {
            list = await _youTubeClient.GetAudiosAsync(sessionContext.Url, ct);
        }
        catch (Exception e)
        {
            questionContext.IsSuccess = false;
            questionContext.Error = e;
            return questionContext;
        }

        if (list.Count == 0)
        {
            questionContext.IsSuccess = false;
            questionContext.Error = new Exception("Could get audios from youtube");
            return questionContext;
        }

        sessionContext.SetAudioItems(_mapper, list);
        
        questionContext.Buttons = sessionContext.GetAudioButtons();
        questionContext.Title = "Audio";
        
        return questionContext;
    }

    #endregion
}