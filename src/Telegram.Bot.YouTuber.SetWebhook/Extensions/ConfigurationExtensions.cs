using System.Reflection;
using Microsoft.Extensions.Configuration;
using Telegram.Bot.YouTuber.Core.Extensions;
using Telegram.Bot.YouTuber.Core.Settings;

namespace Telegram.Bot.YouTuber.SetWebhook.Extensions;

public static class ConfigurationExtensions
{
    public static IConfiguration GetConfiguration(string environmentName)
    {
        ConfigurationBuilder builder = new();
        IConfigurationBuilder configuration = builder
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true);
            
        var assembly = Assembly.GetEntryAssembly();
        if (assembly != null)
        {
            configuration.AddUserSecrets(assembly);
        }

        return configuration.Build();
    }
    
    public static BotConfiguration GetBotConfiguration(this IConfiguration configuration)
    {
        return configuration.GetSection(BotConfiguration.SectionName).Get<BotConfiguration>().AsNotNull(message: "Не найдена конфигурация бота");
    }
}