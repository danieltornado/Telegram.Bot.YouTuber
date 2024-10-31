using AutoMapper;
using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;
using Telegram.Bot.YouTuber.Webhook.Services.Downloading;

namespace Telegram.Bot.YouTuber.Webhook.Services.Sessions;

public sealed class SessionProfile : Profile
{
    public SessionProfile()
    {
        CreateMap<MediaEntity, SessionMediaContext>();
        CreateMap<SessionMediaContext, MediaEntity>();

        CreateMap<SessionContext, SessionEntity>()
            .ForMember(dest => dest.Error, e => e.Ignore());

        CreateMap<VideoInfo, SessionMediaContext>()
            .ForMember(dest => dest.Extension, e => e.MapFrom(src => src.FileExtension));
        
        CreateMap<AudioInfo, SessionMediaContext>()
            .ForMember(dest => dest.Extension, e => e.MapFrom(src => src.FileExtension));
    }
}