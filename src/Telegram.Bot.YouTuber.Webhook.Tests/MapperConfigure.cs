using AutoMapper;
using Telegram.Bot.YouTuber.Webhook.BL.Implementations;

namespace Telegram.Bot.YouTuber.Webhook.Tests;

public sealed class MapperConfigure
{
    private readonly Lazy<IMapper> _mapper;
    
    public MapperConfigure()
    {
        var configuration = new MapperConfiguration(config => config.AddMaps(typeof(CommonProfile)));
        _mapper = new Lazy<IMapper>(() => configuration.CreateMapper());
    }
    
    public IMapper Mapper => _mapper.Value;
}