using AutoMapper;
using Telegram.Bot.Types;
using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;
using Telegram.Bot.YouTuber.Webhook.Extensions;
using Telegram.Bot.YouTuber.Webhook.Services.Downloading;
using Telegram.Bot.YouTuber.Webhook.Services.Questions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Sessions;

public sealed class SessionProfile : Profile
{
    public SessionProfile()
    {
        CreateMap<Update, StartSessionContext>()
            .ForMember(dest => dest.MessageId, e => e.MapFrom((src, _) => src.Message?.MessageId))
            .ForMember(dest => dest.ChatId, e => e.MapFrom((src, _) => src.Message?.Chat.Id))
            .ForMember(dest => dest.Json, e => e.MapFrom(src => src.CreateJson()))
            .ForMember(dest => dest.Url, e => e.MapFrom((src, _) => src.Message?.Text));

        CreateMap<Update, ContinueSessionContext>()
            .ForMember(dest => dest.MessageId, e => e.MapFrom((src, _) => src.Message?.MessageId))
            .ForMember(dest => dest.ChatId, e => e.MapFrom((src, _) => src.Message?.Chat.Id))
            .ForMember(dest => dest.Json, e => e.MapFrom(src => src.CreateJson()));

        CreateMap<SessionMediaContext, QuestionButton>()
            .ForMember(dest => dest.Caption, e => e.MapFrom(src => src.GetQuestionButtonCaption()))
            .ForMember(dest => dest.Data, e => e.MapFrom((src, _) => new QuestionData { Type = src.Type, MediaId = src.Id }.ToCallbackQueryData()));

        CreateMap<MediaEntity, SessionMediaContext>();
        CreateMap<SessionEntity, SessionContext>();

        CreateMap<StartSessionContext, SessionEntity>();

        CreateMap<VideoInfo, MediaEntity>()
            .ForMember(dest => dest.Extension, e => e.MapFrom(src => src.FileExtension))
            .ForMember(dest => dest.Type, e => e.MapFrom(_ => MediaType.Video));

        CreateMap<AudioInfo, MediaEntity>()
            .ForMember(dest => dest.Extension, e => e.MapFrom(src => src.FileExtension))
            .ForMember(dest => dest.Type, e => e.MapFrom(_ => MediaType.Audio));
    }
}