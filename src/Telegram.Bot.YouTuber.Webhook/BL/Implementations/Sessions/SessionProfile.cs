using AutoMapper;
using JetBrains.Annotations;
using Telegram.Bot.Types;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;
using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;

namespace Telegram.Bot.YouTuber.Webhook.BL.Implementations.Sessions;

[UsedImplicitly]
public sealed class SessionProfile : Profile
{
    public SessionProfile()
    {
        CreateMap<Update, StartSessionContext>()
            .ForMember(dst => dst.MessageId, e => e.MapFrom((src, _) => src.Message?.MessageId))
            .ForMember(dst => dst.ChatId, e => e.MapFrom((src, _) => src.Message?.Chat.Id))
            .ForMember(dst => dst.Json, e => e.MapFrom(src => src.CreateJson()))
            .ForMember(dst => dst.Url, e => e.MapFrom((src, _) => src.Message?.Text));

        CreateMap<Update, ContinueSessionContext>()
            .ForMember(dst => dst.MessageId, e => e.MapFrom((src, _) => src.Message?.MessageId))
            .ForMember(dst => dst.ChatId, e => e.MapFrom((src, _) => src.Message?.Chat.Id))
            .ForMember(dst => dst.Json, e => e.MapFrom(src => src.CreateJson()));

        CreateMap<MediaEntity, SessionMediaContext>();
        CreateMap<SessionEntity, SessionContext>();

        CreateMap<StartSessionContext, SessionEntity>();

        CreateMap<VideoInfo, MediaEntity>()
            .ForMember(dst => dst.Extension, e => e.MapFrom(src => src.FileExtension))
            .ForMember(dst => dst.Type, e => e.MapFrom(_ => MediaType.Video));

        CreateMap<AudioInfo, MediaEntity>()
            .ForMember(dst => dst.Extension, e => e.MapFrom(src => src.FileExtension))
            .ForMember(dst => dst.Type, e => e.MapFrom(_ => MediaType.Audio));
    }
}