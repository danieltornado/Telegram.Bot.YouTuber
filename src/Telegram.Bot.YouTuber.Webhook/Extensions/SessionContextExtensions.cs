using AutoMapper;
using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;
using Telegram.Bot.YouTuber.Webhook.Services.Downloading;
using Telegram.Bot.YouTuber.Webhook.Services.Messaging;
using Telegram.Bot.YouTuber.Webhook.Services.Questions;
using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Extensions;

public static class SessionContextExtensions
{
    public static bool HasVideo(this SessionContext context)
    {
        return context.VideoId.HasValue;
    }

    public static bool HasAudio(this SessionContext context)
    {
        return context.AudioId.HasValue;
    }

    public static void SetVideoItems(this SessionContext context, IMapper mapper, IReadOnlyList<VideoInfo> items)
    {
        context.Videos.Clear();
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];

            var mediaContext = mapper.Map<SessionMediaContext>(item);
            mediaContext.Num = i;
            mediaContext.IsSkipped = false;

            context.Videos.Add(mediaContext);
        }
        
        context.Videos.Add(new SessionMediaContext
        {
            Num = items.Count,
            IsSkipped = true
        });
    }

    public static void SetAudioItems(this SessionContext context, IMapper mapper, IReadOnlyList<AudioInfo> items)
    {
        context.Audios.Clear();
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            
            var mediaContext = mapper.Map<SessionMediaContext>(item);
            mediaContext.Num = i;
            mediaContext.IsSkipped = false;

            context.Audios.Add(mediaContext);
        }

        context.Audios.Add(new SessionMediaContext
        {
            Num = items.Count,
            IsSkipped = true
        });
    }

    public static List<QuestionButton> GetVideoButtons(this SessionContext context)
    {
        var buttons = context.Videos
            .Select(e => new QuestionButton
            {
                Caption = e.GetQuestionButtonCaption(),
                Data = new QuestionData { Num = e.Num, Type = MediaType.Video, SessionId = context.Id }.ToCallbackQueryData()
            })
            .ToList();

        return buttons;
    }

    public static List<QuestionButton> GetAudioButtons(this SessionContext context)
    {
        var buttons = context.Audios
            .Select(e => new QuestionButton
            {
                Caption = e.GetQuestionButtonCaption(),
                Data = new QuestionData { Num = e.Num, Type = MediaType.Audio, SessionId = context.Id }.ToCallbackQueryData()
            })
            .ToList();

        return buttons;
    }

    public static void SetVideoAnswer(this SessionContext context, CallbackQueryContext messageContext)
    {
        var video = context.Videos.FirstOrDefault(e => e.Num == messageContext.Num);
        if (video != null)
        {
            context.VideoId = video.Id;
        }

        context.JsonVideo = messageContext.Json;
    }

    public static void SetAudioAnswer(this SessionContext context, CallbackQueryContext messageContext)
    {
        var audio = context.Audios.FirstOrDefault(e => e.Num == messageContext.Num);
        if (audio != null)
        {
            context.AudioId = audio.Id;
        }

        context.JsonAudio = messageContext.Json;
    }

    public static void ApplyExternalContext(this SessionContext sessionContext, QuestionContext context)
    {
        sessionContext.IsSuccess = context.IsSuccess;
        sessionContext.Error = context.Error;
    }

    public static void ApplyExternalContext(this SessionContext sessionContext, DownloadingContext context)
    {
        sessionContext.IsSuccess = context.IsSuccess;
        sessionContext.Error = context.Error;
        sessionContext.FileId = context.Id;
    }
}