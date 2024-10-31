using AutoMapper;
using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;
using Telegram.Bot.YouTuber.Webhook.Extensions;
using VideoLibrary;

namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

public sealed class DownloadingProfile : Profile
{
    public DownloadingProfile()
    {
        // Reverse have to be manually
        CreateMap<DownloadingEntity, DownloadingContext>()
            .ForMember(dest => dest.Error, e => e.Ignore());

        CreateMap<YouTubeVideo, VideoInfo>()
            .ForMember(dest => dest.Format, e => e.MapFrom(src => src.Format.ToString()))
            .ForMember(dest => dest.Quality, e => e.MapFrom(src => src.Resolution.ToString()))
            .ForMember(dest => dest.InternalUrl, e => e.Ignore());
        
        CreateMap<YouTubeVideo, AudioInfo>()
            .ForMember(dest => dest.Format, e => e.MapFrom(src => src.AudioFormat.ToString()))
            .ForMember(dest => dest.Quality, e => e.MapFrom(src => src.AudioBitrate.ToString()))
            .ForMember(dest => dest.InternalUrl, e => e.Ignore());
    }
}