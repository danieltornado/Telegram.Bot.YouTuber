namespace Telegram.Bot.YouTuber.Core.Settings;

public sealed class BotConfiguration
{
    public static readonly string SectionName = "Bot";

    /// <summary>
    /// Токен телеграм-бота
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Url приложения-бота
    /// </summary>
    public string Url { get; set; } = string.Empty;
}