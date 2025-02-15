using AutoMapper;
using JetBrains.Annotations;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;
using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;
using VideoLibrary;
using VideoInfo = Telegram.Bot.YouTuber.Webhook.BL.Abstractions.VideoInfo;

namespace Telegram.Bot.YouTuber.Webhook.BL.Implementations.Downloading;

[UsedImplicitly]
public sealed class DownloadingProfile : Profile
{
    public DownloadingProfile()
    {
        // Reverse have to be manually
        CreateMap<DownloadingEntity, DownloadingContext>();

        CreateMap<YouTubeVideo, VideoInfo>()
            .ForMember(dst => dst.Format, e => e.MapFrom(src => src.Format.ToString()))
            .ForMember(dst => dst.Quality, e => e.MapFrom(src => src.Resolution.ToString()))
            .ForMember(dst => dst.InternalUrl, e => e.Ignore());

        CreateMap<YouTubeVideo, AudioInfo>()
            .ForMember(dst => dst.Format, e => e.MapFrom(src => src.AudioFormat.ToString()))
            .ForMember(dst => dst.Quality, e => e.MapFrom(src => src.AudioBitrate.ToString()))
            .ForMember(dst => dst.InternalUrl, e => e.Ignore());
    }
}