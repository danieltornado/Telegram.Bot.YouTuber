using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Extensions.Logging;
using Telegram.Bot.YouTuber.Core.Settings;
using Telegram.Bot.YouTuber.Webhook.DataAccess;
using Telegram.Bot.YouTuber.Webhook.Extensions;
using Telegram.Bot.YouTuber.Webhook.Services;
using Telegram.Bot.YouTuber.Webhook.Services.Downloading;
using Telegram.Bot.YouTuber.Webhook.Services.Files;
using Telegram.Bot.YouTuber.Webhook.Services.Hosted;
using Telegram.Bot.YouTuber.Webhook.Services.Messaging;
using Telegram.Bot.YouTuber.Webhook.Services.Questions;
using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook;

public static class Startup
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
            .AddEnvironmentVariables();

        builder.Services
            .AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddNLog();
                logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
                LogManager.Configuration = new NLogLoggingConfiguration(builder.Configuration.GetSection("NLog"));
            });

        // Add services to the container.

        if (builder.Environment.IsDevelopment())
        {
            builder.Host.UseDefaultServiceProvider(e =>
            {
                e.ValidateOnBuild = true;
                e.ValidateScopes = true;
            });
            builder.Services.AddControllers().AddControllersAsServices().AddNewtonsoftJson();
        }
        else
        {
            builder.Services.AddControllers().AddNewtonsoftJson();
        }

        builder.Services.AddHealthChecks();
        builder.Services.AddDbContext<AppDbContext>(options => { options.UseNpgsql(builder.Configuration.GetPgsqlConnectionString("YouTuberDb", "Telegram.Bot.YouTuber.Webhook")); });
        builder.Services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; });
        builder.Services.Configure<BotConfiguration>(builder.Configuration.GetSection(BotConfiguration.SectionName));

        builder.Services
            .AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, provider) =>
            {
                var options = provider.GetRequiredService<IOptions<BotConfiguration>>();
                var botConfiguration = options.Value;
                var botToken = botConfiguration.Token;
                return new TelegramBotClient(botToken, httpClient);
            });

        builder.Services
            .AddHttpClient("youtube_delegating_client")
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                HttpClientHandler httpClientHandler = new();
                if (httpClientHandler.SupportsAutomaticDecompression)
                    httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                return httpClientHandler;
            });

        // Warn! Removing LoggingHttpMessageHandlerBuilderFilter only
        builder.Services.RemoveAll<IHttpMessageHandlerBuilderFilter>();

        builder.Services
            .AddTransient<YouTubeClient>();

        builder.Services
            .AddScoped<ITelegramService, TelegramService>()
            .AddScoped<ISessionService, SessionService>()
            .AddScoped<IDelegatingClientFactory, DelegatingClientFactory>()
            .AddScoped<IQuestionService, QuestionService>()
            .AddScoped<IDownloadService, DownloadService>();

        builder.Services
            .AddSingleton<IFileService, FileService>()
            .AddSingleton<IKeyboardService, KeyboardService>()
            .AddSingleton<IDownloadQueueService, DownloadQueueService>()
            .AddSingleton<IQuestionQueueService, QuestionQueueService>()
            .AddSingleton<IMessageService, MessageService>()
            .AddSingleton<IStickService, StickService>();

        builder.Services.AddHostedService<DownloadHostedService>();
        builder.Services.AddHostedService<CleanHostedService>();
        builder.Services.AddHostedService<QuestionHostedService>();

        return builder;
    }

    public static WebApplication ConfigurePipeline(this WebApplicationBuilder builder)
    {
        var app = builder.Build();

        // Proxing nginx.
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        // microsoft:
        //  Deactivate HTTPS Redirection Middleware in the Development environment (Program.cs)
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseRouting();

        // twin configuration
        string? pathBase = app.Configuration["PathBase"];
        if (!string.IsNullOrWhiteSpace(pathBase))
            app.UsePathBase(pathBase);

        app.UseExceptionHandler(new ExceptionHandlerOptions { ExceptionHandler = HandleException });
        app.UseHealthChecks("/healthz");
        app.MapControllers();

        return app;
    }

    public static async Task MigrateDbAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
    }

    private static Task HandleException(HttpContext context)
    {
        const string internalServerErrorText = "Internal server error";

        var logger = context.RequestServices.GetRequiredService<ILogger<HttpContext>>();
        var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();

        string responseMessage = internalServerErrorText;

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = 500;

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;
        if (exception != null)
        {
            if (!env.IsEnvironment("Production"))
                responseMessage = exception.ToString();
        }

        logger.LogError(exception, "Message={Message}; Path={Path}; Method={Method}", internalServerErrorText, context.Request.Path.ToString(), context.Request.Method);

        var response = new { message = responseMessage };
        return context.Response.WriteAsJsonAsync(response);
    }
}