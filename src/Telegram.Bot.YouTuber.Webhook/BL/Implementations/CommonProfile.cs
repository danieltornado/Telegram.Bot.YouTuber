using AutoMapper;
using JetBrains.Annotations;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Questions;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;
using Telegram.Bot.YouTuber.Webhook.BL.Implementations.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.BL.Implementations;

[UsedImplicitly]
public sealed class CommonProfile : Profile
{
    public CommonProfile()
    {
        CreateMap<SessionMediaContext, QuestionButton>()
            .ForMember(dst => dst.Caption, e => e.MapFrom(src => src.GetQuestionButtonCaption()))
            .ForMember(dst => dst.Data, e => e.MapFrom((src, _) => new QuestionData { Type = src.Type, MediaId = src.Id }.ToCallbackQueryData()));
    }
}