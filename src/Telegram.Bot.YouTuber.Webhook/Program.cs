using Serilog;

namespace Telegram.Bot.YouTuber.Webhook;

public class Program
{
    public static async Task Main(string[] args)
    {
        // https://github.com/serilog/serilog-aspnetcore
        // https://github.com/serilog/serilog-settings-configuration
        // https://github.com/serilog/serilog/wiki/Formatting-Output
        // https://github.com/serilog/serilog-aspnetcore/tree/dev/samples/Sample
        // https://github.com/serilog/serilog-sinks-rollingfile
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            var app = WebApplication
                .CreateBuilder(args)
                .ConfigureServices()
                .Build()
                .ConfigurePipeline();

            await app.MigrateDbAsync();
            await app.RunAsync();
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Stopped program because of exception");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}