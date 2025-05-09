namespace Telegram.Bot.YouTuber.Webhook.Extensions;

public static class HostEnvironmentExtensions
{
    public static bool IsTest(this IHostEnvironment hostEnvironment)
    {
        return hostEnvironment.IsEnvironment("Test");
    }
}