using System.Data.Common;
using System.Net.Http.Headers;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot.Types;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Downloading;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Questions;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Queues;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;
using Telegram.Bot.YouTuber.Webhook.BL.Implementations;
using Telegram.Bot.YouTuber.Webhook.BL.Implementations.Downloading;

namespace Telegram.Bot.YouTuber.Webhook.Tests;

public static class WebApplicationFactoryExtensions
{
    public const string ENVIRONMENT_TEST = "Test";

    public const string DATABASE_NAME_1 = ":memory:1";
    public const string DATABASE_NAME_2 = ":memory:2";
    public const string DATABASE_NAME_3 = ":memory:3";

    public static void RemoveAll<TRegistrationType>(this IServiceCollection services)
        where TRegistrationType : class
    {
        services.RemoveAll(typeof(TRegistrationType));
    }

    /// <summary>
    /// Sets using Sqlite in memory
    /// </summary>
    /// <param name="optionsBuilder"></param>
    /// <param name="databaseName"></param>
    public static DbConnection UseSqliteInMemory(this DbContextOptionsBuilder optionsBuilder, string databaseName)
    {
        SqliteConnectionStringBuilder connectionStringBuilder = new();
        connectionStringBuilder.Mode = SqliteOpenMode.Memory;
        connectionStringBuilder.Cache = SqliteCacheMode.Shared;
        connectionStringBuilder.DataSource = databaseName;

        var connectionString = connectionStringBuilder.ToString();
        var connection = new SqliteConnection(connectionString);
        connection.Open();

        optionsBuilder.UseSqlite(connection);

        return connection;
    }

    /// <summary>
    /// Sends post-request
    /// </summary>
    /// <param name="app"></param>
    /// <param name="url"></param>
    /// <param name="body"></param>
    /// <typeparam name="TEntryPoint"></typeparam>
    /// <typeparam name="TBody"></typeparam>
    /// <returns></returns>
    public static Task<HttpResponseMessage> PostAsync<TEntryPoint, TBody>(this WebApplicationFactory<TEntryPoint> app, string url, TBody body)
        where TEntryPoint : class
    {
        string bodyStr = System.Text.Json.JsonSerializer.Serialize(body, JsonBotAPI.Options);

        HttpRequestMessage request = new(HttpMethod.Post, url);
        request.Content = new StringContent(bodyStr, new MediaTypeHeaderValue("application/json"));

        var httpClient = app.CreateClient();

        return httpClient.SendAsync(request);
    }

    /// <summary>
    /// Stops all the hosted services and waits stopping
    /// </summary>
    /// <param name="app"></param>
    /// <typeparam name="TEntryPoint"></typeparam>
    public static async Task StopHostedServicesAsync<TEntryPoint>(this WebApplicationFactory<TEntryPoint> app)
        where TEntryPoint : class
    {
        foreach (var hostedService in app.Services.GetServices<IHostedService>())
        {
            await hostedService.StopAsync(CancellationToken.None);
        }
    }

    /// <summary>
    /// Executes <paramref name="action"/> on database (inside scope)
    /// </summary>
    /// <param name="app"></param>
    /// <param name="action"></param>
    /// <typeparam name="TEntryPoint"></typeparam>
    /// <typeparam name="TDbContext"></typeparam>
    public static async Task ExecuteDbInScope<TEntryPoint, TDbContext>(this WebApplicationFactory<TEntryPoint> app, Func<TDbContext, Task> action)
        where TEntryPoint : class
        where TDbContext : DbContext
    {
        await using var scope = app.Services.CreateAsyncScope();
        await using var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await action(db);
    }

    /// <summary>
    /// Injects <paramref name="waitCompletion"/> into <see cref="IMessageHandling.HandleMessageAsync"/>
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="messageHandlingMock"></param>
    /// <param name="waitCompletion"></param>
    public static void InjectWaitingInMessageHandling(this IServiceProvider serviceProvider, Mock<IMessageHandling> messageHandlingMock, TaskCompletionSource waitCompletion)
    {
        messageHandlingMock
            .Setup(e => e.HandleMessageAsync(It.IsAny<Update>(), It.IsAny<RequestContext>(), It.IsAny<CancellationToken>()))
            .Returns<Update, RequestContext, CancellationToken>(async (update, context, ct) =>
            {
                // Overrides default realization by inserting waiting operation at the end 

                try
                {
                    // ReSharper disable once AccessToDisposedClosure
                    using var scope = serviceProvider.CreateScope();
                    var serviceProviderScoped = scope.ServiceProvider;
                    var originalHandling = new MessageHandling(
                        sessionService: serviceProviderScoped.GetRequiredService<ISessionService>(),
                        telegramService: serviceProviderScoped.GetRequiredService<ITelegramService>(),
                        youTubeClient: serviceProviderScoped.GetRequiredService<IYouTubeClient>(),
                        keyboardService: serviceProviderScoped.GetRequiredService<IKeyboardService>(),
                        sessionQueueService: serviceProviderScoped.GetRequiredService<ISessionsQueueService>(),
                        mapper: serviceProviderScoped.GetRequiredService<IMapper>(),
                        logger: serviceProviderScoped.GetRequiredService<ILogger<MessageHandling>>());

                    await originalHandling.HandleMessageAsync(update, context, ct);
                }
                finally
                {
                    // can finish operation
                    waitCompletion.SetResult();
                }
            });
    }

    /// <summary>
    /// Injects <paramref name="waitCompletion"/> into <see cref="IMessageHandling.HandleMessageAsync"/>
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="workerInstanceMock"></param>
    /// <param name="waitCompletion"></param>
    public static void InjectWaitingInWorkerInstance(this IServiceProvider serviceProvider, Mock<IWorkerInstance> workerInstanceMock, TaskCompletionSource waitCompletion)
    {
        workerInstanceMock
            .Setup(e => e.ProcessAsync(It.IsAny<SessionContext>(), It.IsAny<CancellationToken>()))
            .Returns<SessionContext, CancellationToken>(async (sessionContext, ct) =>
            {
                // Overrides default realization by inserting waiting operation at the end 

                try
                {
                    // ReSharper disable once AccessToDisposedClosure
                    await using var scope = serviceProvider.CreateAsyncScope();
                    var serviceProviderScoped = scope.ServiceProvider;
                    var originalHandling = new WorkerInstance(
                        serviceProvider: serviceProviderScoped,
                        freeWorkersService: serviceProviderScoped.GetRequiredService<IFreeWorkersService>(),
                        fileService: serviceProviderScoped.GetRequiredService<IFileService>(),
                        logger: serviceProviderScoped.GetRequiredService<ILogger<WorkerInstance>>());

                    await originalHandling.ProcessAsync(sessionContext, ct);
                }
                finally
                {
                    // can finish operation
                    waitCompletion.SetResult();
                }
            });
    }

    /// <summary>
    /// Standard waiting with constant timeout (1 minute)
    /// </summary>
    /// <param name="waitCompletion"></param>
    /// <returns></returns>
    public static Task WaitWithTimeout(this Task waitCompletion)
    {
        return waitCompletion.WaitAsync(TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// Use environment for tests
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IWebHostBuilder UseTestEnvironment(this IWebHostBuilder builder)
    {
        return builder.UseEnvironment(ENVIRONMENT_TEST);
    }

    /// <summary>
    /// Use appsettings.Test.json file
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static void AddAppSettingsTestJson(this IConfigurationBuilder builder)
    {
        builder.AddJsonFile("appsettings.Test.json");
    }
}