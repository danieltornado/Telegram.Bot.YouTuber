using System.Data.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot.Types;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Downloading;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Media;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Queues;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;
using Telegram.Bot.YouTuber.Webhook.BL.Implementations;
using Telegram.Bot.YouTuber.Webhook.DataAccess;
using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;
using Telegram.Bot.YouTuber.Webhook.Services.Hosted;
using Xunit.Abstractions;

namespace Telegram.Bot.YouTuber.Webhook.Tests.Controllers.MessageController;

public sealed class DownloadSessionTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public DownloadSessionTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// When application receives a third message with an audio selection, it has to download 
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task WhenReceivedAudioSelection_Update_ShouldDownloadSession()
    {
        // -------ARRANGE-------
        long chatId = 123;
        int messageId = 456;
        long senderId = 789;
        Guid videoId = Guid.NewGuid();
        Guid audioId = Guid.NewGuid();
        Guid fileId = Guid.NewGuid();

        DbConnection? dbConnection = null;

        Mock<IYouTubeClient> youTubeClientMock = new(); // without moq because it is used inside IDownloadingClient
        Mock<ITelegramService> telegramServiceMock = new();
        Mock<IMessageHandling> messageHandlingMock = new();
        Mock<IStickService> stickServiceMock = new(); // without moq because it is used inside IDownloadingClient
        Mock<IFileService> fileServiceMock = new(); // without moq because it is used inside IDownloadingClient
        Mock<IDownloadingClient> downloadingClientMock = new();
        // single worker
        Mock<IWorkerInstance> workerInstanceMock = new();

        await using var app = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder
                    .UseTestEnvironment()
                    .ConfigureAppConfiguration(configuration =>
                    {
                        configuration.Sources.Clear();
                        configuration.AddAppSettingsTestJson();
                    })
                    .ConfigureServices(services =>
                    {
                        // Moq an external interaction
                        services.RemoveAll<DbContextOptions<AppDbContext>>();
                        services.AddDbContext<AppDbContext>(options => dbConnection = options.UseSqliteInMemory(WebApplicationFactoryExtensions.DATABASE_NAME_3));

                        // Moq an external interaction
                        services.RemoveAll<IYouTubeClient>();
                        services.AddTransient<IYouTubeClient>(_ => youTubeClientMock.Object);

                        // Moq an external interaction
                        services.RemoveAll<ITelegramBotClient>();
                        services.RemoveAll<ITelegramService>();
                        services.AddScoped<ITelegramService>(_ => telegramServiceMock.Object);

                        // Moq an external interaction
                        services.RemoveAll<IFileService>();
                        services.AddTransient<IFileService>(_ => fileServiceMock.Object);

                        // Moq an external interaction
                        services.RemoveAll<IStickService>();
                        services.AddTransient<IStickService>(_ => stickServiceMock.Object);

                        // Moq an external interaction
                        services.RemoveAll<IDownloadingClient>();
                        services.AddTransient<IDownloadingClient>(_ => downloadingClientMock.Object);

                        // Overrides behavior special for case
                        services.RemoveAll<IMessageHandling>();
                        services.AddScoped<IMessageHandling>(_ => messageHandlingMock.Object);

                        // Cleaning is not tested
                        services.RemoveAll<IHostedService>();
                        services.AddHostedService<DownloadHostedService>();
                    })
                    .ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddProvider(new XunitLoggerProvider(_testOutputHelper));
                    });
            });

        // Block the test executing while message handling is still working
        TaskCompletionSource waitHandleMessageEnd = new();

        // moq handling
        app.Services.InjectWaitingInMessageHandling(messageHandlingMock, waitHandleMessageEnd);

        // source data
        QuestionData questionData = new()
        {
            Type = MediaType.Audio,
            MediaId = audioId
        };

        // emulated message
        Update telegramMessage = new()
        {
            Id = 1,
            CallbackQuery = new()
            {
                Id = Guid.NewGuid().ToString(),
                From = new()
                {
                    Id = senderId,
                    IsBot = false,
                    Username = "johndoe",
                    FirstName = "John",
                    LastName = "Doe"
                },
                ChatInstance = new("chat"),
                Data = questionData.ToCallbackQueryData(),
            }
        };

        // moq session
        await app.ExecuteDbInScope<Program, AppDbContext>(async context =>
        {
            await context.Database.EnsureCreatedAsync();

            // seed data
            await context.Sessions.AddAsync(new SessionEntity
            {
                ChatId = chatId,
                MessageId = messageId,
                VideoId = videoId,
                Media =
                [
                    new()
                    {
                        // Video
                        Id = videoId,
                        Type = MediaType.Video,
                        Extension = "mp4",
                        Format = "mp4",
                        Quality = "720p"
                    },

                    new()
                    {
                        // Audio
                        Id = audioId,
                        Type = MediaType.Audio,
                        Extension = "mp3",
                        Format = "aac",
                        Quality = "480p"
                    }

                ],

                // other fields do not matter
            });
            await context.SaveChangesAsync();
        });

        // wait workerInstance
        TaskCompletionSource waitProcessEnd = new();

        // inject waiting into workerInstance
        app.Services.InjectWaitingInWorkerInstance(workerInstanceMock, waitProcessEnd);

        // manual registering workerInstance
        var freeWorkersService = app.Services.GetRequiredService<IFreeWorkersService>();
        await freeWorkersService.QueueAsync(workerInstanceMock.Object, CancellationToken.None);

        // moq
        downloadingClientMock
            .Setup(e => e.DownloadAsync(It.IsAny<Guid>(), It.IsAny<SessionMediaContext>(), It.IsAny<SessionMediaContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileId);

        // -------ACT----------
        var response = await app.PostAsync("/api/message/update", telegramMessage);

        // ------ASSERT--------

        // check: response is ok
        response.Should().NotBeNull();

        if (response.IsSuccessStatusCode is false)
        {
            _testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Fail(response.ReasonPhrase);
        }

        // waiting completing operation
        await waitHandleMessageEnd.Task.WaitWithTimeout();
        await waitProcessEnd.Task.WaitWithTimeout();

        // check: session has got video answer
        await app.ExecuteDbInScope((AppDbContext db) =>
        {
            db.Sessions.Should().Contain(e => e.ChatId == chatId && e.MessageId == messageId && e.VideoId.HasValue);
            return Task.CompletedTask;
        });

        // check: audio question has been sent
        telegramServiceMock
            .Verify(e => e.SendMessageAsync(chatId, messageId, $"http://localhost/api/files/{fileId:D}", It.IsAny<CancellationToken>()), Times.Once);

        // stopping
        await app.StopHostedServicesAsync();

        // close db connection
        if (dbConnection is not null)
            await dbConnection.CloseAsync();
    }
}