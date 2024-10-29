using System.Web;
using Npgsql;

namespace Telegram.Bot.YouTuber.Webhook.Extensions;

public static class ConfigurationExtensions
{
    public static string GetPgsqlConnectionString(this IConfiguration configuration, string connectionStringName, string applicationName)
    {
        var stringUri = configuration.GetConnectionString(connectionStringName);
        if (stringUri is null)
            throw new InvalidOperationException("Отсутствует строка подключения");
        
        Uri factUri = new(stringUri);
        string[] userInfo = factUri.UserInfo.Split(':');
        
        NpgsqlConnectionStringBuilder stringBuilder = new()
        {
            Host = factUri.Host,
            Port = factUri.Port,
            Username = userInfo[0],
            Password = HttpUtility.UrlDecode(userInfo[1]),
            Database = factUri.LocalPath.TrimStart('/'),
            IncludeErrorDetail = true,
            ApplicationName = applicationName
        };
        
        return stringBuilder.ToString();
    }
}