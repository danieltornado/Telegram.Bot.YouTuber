using System.Diagnostics;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot.YouTuber.Core.Settings;
using Telegram.Bot.YouTuber.Webhook.DataAccess;
using Telegram.Bot.YouTuber.Webhook.Extensions;
using Telegram.Bot.YouTuber.Webhook.Services.Hosted;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Downloading;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Media;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Questions;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Queues;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;
using Telegram.Bot.YouTuber.Webhook.BL.Implementations;
using Telegram.Bot.YouTuber.Webhook.BL.Implementations.Downloading;
using Telegram.Bot.YouTuber.Webhook.BL.Implementations.Media;
using Telegram.Bot.YouTuber.Webhook.BL.Implementations.Questions;
using Telegram.Bot.YouTuber.Webhook.BL.Implementations.Queues;
using Telegram.Bot.YouTuber.Webhook.BL.Implementations.Sessions;
using Telegram.Bot.YouTuber.Webhook.Services;

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

        // Can don't clear providers
        builder.Services.AddSerilog(cfg => cfg.ReadFrom.Configuration(builder.Configuration));

        // Add services to the container.

        if (builder.Environment.IsDevelopment())
        {
            builder.Host.UseDefaultServiceProvider(e =>
            {
                e.ValidateOnBuild = true;
                e.ValidateScopes = true;
            });
            builder.Services.AddControllers().AddControllersAsServices();
        }
        else
        {
            builder.Services.AddControllers();
        }

        builder.Services.AddHealthChecks();
        builder.Services.AddDbContext<AppDbContext>(options => { options.UseNpgsql(builder.Configuration.GetPgsqlConnectionString("YouTuberDb", "Telegram.Bot.YouTuber.Webhook")); });
        builder.Services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; });
        builder.Services.Configure<BotConfiguration>(builder.Configuration.GetSection(BotConfiguration.SectionName));

        builder.Services
            .ConfigureTelegramBotMvc()
            .AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, provider) =>
            {
                var botConfiguration = provider.GetRequiredService<IOptions<BotConfiguration>>();
                var telegramBotClientOptions = new TelegramBotClientOptions(token: botConfiguration.Value.Token);
                return new TelegramBotClient(options: telegramBotClientOptions, httpClient: httpClient);
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

        // https://www.milanjovanovic.tech/blog/problem-details-for-aspnetcore-apis
        // Adds services for using Problem Details format
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance =
                    $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

                Activity? activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
                context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
            };
        });

        // https://www.milanjovanovic.tech/blog/problem-details-for-aspnetcore-apis
        builder.Services.AddExceptionHandler<CustomExceptionHandler>();

        // Warn! Removing LoggingHttpMessageHandlerBuilderFilter only
        builder.Services.RemoveAll<IHttpMessageHandlerBuilderFilter>();

        builder.Services
            .AddAutoMapper(typeof(Startup));

        builder.Services
            .AddTransient<IYouTubeClient, YouTubeClient>()
            .AddTransient<IWorkerInstance, WorkerInstance>();

        builder.Services
            .AddScoped<ITelegramService, TelegramService>()
            .AddScoped<ISessionService, SessionService>()
            .AddScoped<IDelegatingClientFactory, DelegatingClientFactory>()
            .AddScoped<IDownloadingClient, DownloadingClient>()
            .AddScoped<IDownloadingService, DownloadingService>()
            .AddScoped<IStickService, StickService>()
            .AddScoped<IMessageHandling, MessageHandling>()
            .AddScoped<ICustomLinkGenerator, CustomLinkGenerator>();

        builder.Services
            .AddSingleton<IFileService, FileService>()
            .AddSingleton<IKeyboardService, KeyboardService>()
            .AddSingleton<ISessionsQueueService, SessionsQueueService>()
            .AddSingleton<IFreeWorkersService, FreeWorkersService>();

        builder.Services
            .AddHostedService<DownloadHostedService>()
            .AddHostedService<CleanHostedService>()
            .AddHostedService<WorkersRegistrationHostedService>();

        return builder;
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        // Proxing nginx.
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });
        
        // Smart logging
        app.UseSerilogRequestLogging();

        // microsoft:
        //  Deactivate HTTPS Redirection Middleware in the Development environment (Program.cs)
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseRouting();

        // https://www.milanjovanovic.tech/blog/problem-details-for-aspnetcore-apis
        // Converts unhandled exceptions into Problem Details responses
        app.UseExceptionHandler();

        app.UseHealthChecks("/healthz");
        app.MapControllers();

        return app;
    }

    /// <summary>
    /// Executes migrations
    /// </summary>
    /// <param name="app"></param>
    public static Task MigrateDbAsync(this WebApplication app)
    {
        return app.Services.MigrateDbAsync();
    }

    /// <summary>
    /// Executes migrations
    /// </summary>
    /// <param name="serviceProvider"></param>
    private static async Task MigrateDbAsync(this IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
    }
}