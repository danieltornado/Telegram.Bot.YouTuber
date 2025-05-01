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
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Downloading;
using Telegram.Bot.YouTuber.Webhook.DataAccess;
using Xunit;
using Xunit.Abstractions;

namespace Telegram.Bot.YouTuber.Webhook.Tests.Controllers.MessageController;

public sealed class StartSessionTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public StartSessionTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// When application receives a first message with an url, it has to send a video question 
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task WhenReceivedUrl_Update_ShouldStartSessionAndSendVideoQuestion()
    {
        // -------ARRANGE-------
        long chatId = 123;
        int messageId = 456;
        long senderId = 789;

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
                        services.AddDbContext<AppDbContext>(options => dbConnection = options.UseSqliteInMemory(WebApplicationFactoryExtensions.DATABASE_NAME_1));

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

        // emulate getting youtube metadata
        youTubeClientMock
            .Setup(e => e.GetMetadataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                Video: new List<VideoInfo>
                {
                    new()
                    {
                        Format = "mp4",
                        Quality = "720p",
                        Title = "Title",
                        ContentLength = 1024,
                        FileExtension = "mp4",
                        InternalUrl = "https://..."
                    }
                },
                Audio: new List<AudioInfo>
                {
                    new()
                    {
                        Format = "aac",
                        Quality = "480p",
                        Title = "Title",
                        ContentLength = 1024,
                        FileExtension = "mp3",
                        InternalUrl = "https://..."
                    }
                }));

        // emulated message
        Update telegramMessage = new()
        {
            Id = 1,
            Message = new()
            {
                Id = messageId,
                Chat = new()
                {
                    Id = chatId,
                    Type = ChatType.Private,
                    FirstName = "John",
                    LastName = "Doe"
                },
                From = new()
                {
                    Id = senderId,
                    IsBot = false,
                    Username = "johndoe",
                    FirstName = "John",
                    LastName = "Doe"
                },
                Text = "https://youtu.be/uiXS232iIu1234"
            }
        };

        // -------ACT----------
        var response = await app.PostAsync("/api/messages/update", telegramMessage);

        // -------ASSERT-------

        // check: response is ok
        response.Should().NotBeNull();
        response.IsSuccessStatusCode.Should().BeTrue();

        // waiting completing operation
        await waitHandleMessageEnd.Task.WaitWithTimeout();

        // check: session has been created
        await app.ExecuteDbInScope((AppDbContext db) =>
        {
            db.Sessions
                .Should()
                .Contain(e =>
                    e.ChatId == chatId && e.MessageId == messageId && e.Url == telegramMessage.Message.Text);

            return Task.CompletedTask;
        });

        // check: video question has been sent
        telegramServiceMock
            .Verify(e => e.SendKeyboardAsync(chatId, messageId, "Video", It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()), Times.Once);

        // close db connection
        if (dbConnection is not null)
            await dbConnection.CloseAsync();
    }
}