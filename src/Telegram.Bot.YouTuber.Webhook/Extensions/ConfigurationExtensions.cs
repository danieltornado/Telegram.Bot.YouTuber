using System.Web;
using Npgsql;

namespace Telegram.Bot.YouTuber.Webhook.Extensions;

public static class ConfigurationExtensions
{
    /// <summary>
    /// Gets decoded postgres connection string
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="connectionStringName"></param>
    /// <param name="applicationName"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string GetPgsqlConnectionString(this IConfiguration configuration, string connectionStringName, string applicationName)
    {
        var stringUri = configuration.GetConnectionString(connectionStringName);
        if (stringUri is null)
            throw new InvalidOperationException("Connection string is missing");

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

    /// <summary>
    /// Gets a setting "PathFile"
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static string GetPathFile(this IConfiguration configuration)
    {
        return configuration["PathFile"] ?? throw new InvalidOperationException("Path file is missing");
    }

    /// <summary>
    /// Gets a setting "WorkersCount"
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    /// <exception cref="InvalidProgramException"></exception>
    public static int GetWorkersCount(this IConfiguration configuration)
    {
        var count = configuration.GetValue<int>("WorkersCount");
        if (count <= 0)
            throw new InvalidProgramException("WorkersCount must be greater than 0");

        return count;
    }
}