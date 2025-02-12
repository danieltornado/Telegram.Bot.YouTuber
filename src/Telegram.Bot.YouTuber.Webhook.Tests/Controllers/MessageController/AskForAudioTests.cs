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
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Downloading;
using Telegram.Bot.YouTuber.Webhook.BL.Implementations;
using Telegram.Bot.YouTuber.Webhook.DataAccess;
using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;
using Xunit.Abstractions;

namespace Telegram.Bot.YouTuber.Webhook.Tests.Controllers.MessageController;

public sealed class AskForAudioTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public AskForAudioTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// When application receives a second message with a video selection, it has to send a video question 
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task WhenReceivedVideoSelection_Update_ShouldSendAudioQuestion()
    {
        // -------ARRANGE-------
        long chatId = 123;
        int messageId = 456;
        long senderId = 789;
        Guid videoId = Guid.NewGuid();
        Guid audioId = Guid.NewGuid();

        DbConnection? dbConnection = null;

        Mock<IYouTubeClient> youTubeClientMock = new();
        Mock<ITelegramService> telegramServiceMock = new();

        Mock<IMessageHandling> messageHandlingMock = new();

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
                        services.AddDbContext<AppDbContext>(options => dbConnection = options.UseSqliteInMemory(WebApplicationFactoryExtensions.DATABASE_NAME_2));

                        // Moq an external interaction
                        services.RemoveAll<IYouTubeClient>();
                        services.AddTransient<IYouTubeClient>(_ => youTubeClientMock.Object);

                        // Moq an external interaction
                        services.RemoveAll<ITelegramBotClient>();
                        services.RemoveAll<ITelegramService>();
                        services.AddScoped<ITelegramService>(_ => telegramServiceMock.Object);

                        // Moq an external interaction
                        services.RemoveAll<IFileService>();

                        // Overrides behavior special for case
                        services.RemoveAll<IMessageHandling>();
                        services.AddScoped<IMessageHandling>(_ => messageHandlingMock.Object);

                        // Cleaning is not tested
                        services.RemoveAll<IHostedService>();
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
            Type = MediaType.Video,
            MediaId = videoId
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
                Media = new List<MediaEntity>
                {
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
                    },
                },

                // other fields do not matter
            });
            await context.SaveChangesAsync();
        });

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

        // check: session has got video answer
        await app.ExecuteDbInScope((AppDbContext db) =>
        {
            db.Sessions.Should().Contain(e => e.ChatId == chatId && e.MessageId == messageId && e.VideoId == videoId && e.AudioId == null);
            return Task.CompletedTask;
        });

        // check: audio question has been sent
        telegramServiceMock
            .Verify(e => e.SendKeyboardAsync(chatId, messageId, "Audio", It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()), Times.Once);

        // close db connection
        if (dbConnection is not null)
            await dbConnection.CloseAsync();
    }
}