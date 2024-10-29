// Early init of NLog to allow startup and exception logging, before host is built

using NLog;
using NLog.Web;
using Telegram.Bot.YouTuber.Webhook;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var app = WebApplication
        .CreateBuilder(args)
        .ConfigureServices()
        .ConfigurePipeline();

    await app.MigrateDbAsync();
    await app.RunAsync();
}
catch (Exception e)
{
    // NLog: catch setup errors
    logger.Error(e, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    LogManager.Shutdown();
}