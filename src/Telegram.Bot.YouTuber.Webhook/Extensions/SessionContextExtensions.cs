using Newtonsoft.Json;
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

    public static void SetVideoItems(this SessionContext context, IReadOnlyList<VideoInfo> items)
    {
        context.Videos.Clear();
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];

            context.Videos.Add(new SessionMediaContext
            {
                Extension = item.FileExtension,
                Format = item.Format,
                Quality = item.Quality,
                InternalUrl = item.InternalUrl,
                Title = item.Title,
                Num = i
            });
        }
    }

    public static void SetAudioItems(this SessionContext context, IReadOnlyList<AudioInfo> items)
    {
        context.Audios.Clear();
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];

            context.Audios.Add(new SessionMediaContext
            {
                Extension = item.FileExtension,
                Format = item.Format,
                Quality = item.Quality,
                InternalUrl = item.InternalUrl,
                Title = item.Title,
                Num = i
            });
        }
    }

    public static List<QuestionButton> GetVideoButtons(this SessionContext context)
    {
        var buttons = context.Videos
            .Select(e => new QuestionButton
            {
                Caption = $"{e.Format}  {e.Quality}",
                Data = JsonConvert.SerializeObject(new QuestionData { Num = e.Num, Type = MediaType.Video, SessionId = context.Id })
            })
            .ToList();

        return buttons;
    }

    public static List<QuestionButton> GetAudioButtons(this SessionContext context)
    {
        var buttons = context.Audios
            .Select(e => new QuestionButton
            {
                Caption = $"{e.Format}  {e.Quality}",
                Data = JsonConvert.SerializeObject(new QuestionData { Num = e.Num, Type = MediaType.Audio, SessionId = context.Id })
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
}