using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Downloading;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Questions;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Queues;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;
using Telegram.Bot.YouTuber.Webhook.BL.Implementations;

namespace Telegram.Bot.YouTuber.Webhook.Tests.BL;

public sealed class MessageHandlingTests
{
    private readonly MessageHandling _messageHandling;

    private readonly Mock<ISessionService> _sessionServiceMock = new();
    private readonly Mock<ITelegramService> _telegramServiceMock = new();
    private readonly Mock<IYouTubeClient> _youTubeClientMock = new();
    private readonly Mock<IKeyboardService> _keyboardServiceMock = new();
    private readonly Mock<ISessionsQueueService> _sessionQueueServiceMock = new();

    public MessageHandlingTests()
    {
        var mapperConfig = new MapperConfiguration(config => config.AddMaps(typeof(CommonProfile)));

        _messageHandling = new MessageHandling(
            sessionService: _sessionServiceMock.Object,
            telegramService: _telegramServiceMock.Object,
            youTubeClient: _youTubeClientMock.Object,
            keyboardService: _keyboardServiceMock.Object,
            sessionQueueService: _sessionQueueServiceMock.Object,
            mapper: mapperConfig.CreateMapper(),
            logger: NullLogger<MessageHandling>.Instance);
    }

    [Fact]
    public async Task WhenMessageHasStartText_HandeMessage_ShouldSendWelcomeMessage()
    {
        // arrange
        long chatId = 123;
        int messageId = 456;

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
                    Id = 4,
                    IsBot = false,
                    Username = "johndoe",
                    FirstName = "John",
                    LastName = "Doe"
                },
                Text = "/start"
            }
        };

        // emulated http context
        RequestContext requestContext = new()
        {
            Host = HostString.FromUriComponent("localhost:5000"),
            Scheme = "http",
            PathBase = "/pathBase"
        };

        _sessionServiceMock
            .Setup(e => e.StartSessionAsync(It.IsAny<StartSessionContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SessionContext());

        // act
        await _messageHandling.HandleMessageAsync(telegramMessage, requestContext, CancellationToken.None);

        // assert
        _telegramServiceMock
            .Verify(e => e.SendWelcomeMessageAsync(chatId, It.IsAny<CancellationToken>()), Times.Once);

        _telegramServiceMock
            .Verify(e => e.SendInvalidUrlMessageAsync(chatId, messageId, It.IsAny<CancellationToken>()), Times.Never);

        _sessionServiceMock
            .Verify(e => e.StartSessionAsync(It.IsAny<StartSessionContext>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenMessageHasInvalidUrl_HandeMessage_ShouldSendInvalidUrlMessage()
    {
        // arrange
        long chatId = 123;
        int messageId = 456;

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
                    Id = 4,
                    IsBot = false,
                    Username = "johndoe",
                    FirstName = "John",
                    LastName = "Doe"
                },
                Text = "123"
            }
        };

        // emulated http context
        RequestContext requestContext = new()
        {
            Host = HostString.FromUriComponent("localhost:5000"),
            Scheme = "http",
            PathBase = "/pathBase"
        };

        _sessionServiceMock
            .Setup(e => e.StartSessionAsync(It.IsAny<StartSessionContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SessionContext());

        // act
        await _messageHandling.HandleMessageAsync(telegramMessage, requestContext, CancellationToken.None);

        // assert
        _telegramServiceMock
            .Verify(e => e.SendWelcomeMessageAsync(chatId, It.IsAny<CancellationToken>()), Times.Never);

        _telegramServiceMock
            .Verify(e => e.SendInvalidUrlMessageAsync(chatId, messageId, It.IsAny<CancellationToken>()), Times.Once);

        _sessionServiceMock
            .Verify(e => e.StartSessionAsync(It.IsAny<StartSessionContext>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}