using Telegram.Bot.YouTuber.Webhook.BL.Abstractions;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Downloading;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Queues;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;
using Telegram.Bot.YouTuber.Webhook.DataAccess.Exceptions;

namespace Telegram.Bot.YouTuber.Webhook.BL.Implementations.Downloading;

/// <inheritdoc cref="IWorkerInstance" />
internal sealed class WorkerInstance : IWorkerInstance
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFreeWorkersService _workersQueueService;
    private readonly IFileService _fileService;
    private readonly ILogger<WorkerInstance> _logger;
    private readonly int _workerId;

    public WorkerInstance(IServiceProvider serviceProvider, IFreeWorkersService freeWorkersService, IFileService fileService, ILogger<WorkerInstance> logger)
    {
        _serviceProvider = serviceProvider;
        _workersQueueService = freeWorkersService;
        _fileService = fileService;
        _logger = logger;
        _workerId = freeWorkersService.GenerateUniqueWorkerId();
    }

    #region Implementation of IWorkerInstance

    public async Task ProcessAsync(SessionContext context, CancellationToken ct)
    {
        _logger.LogInformation("WorkerInstance started work: {WorkerId}", _workerId);

        using var scope = _serviceProvider.CreateScope();

        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
        var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramService>();
        var downloadingClient = scope.ServiceProvider.GetRequiredService<IDownloadingClient>();

        try
        {
            var list = await sessionService.GetMediaAsync([context.VideoId.GetValueOrDefault(), context.AudioId.GetValueOrDefault()], ct);

            var video = list.FirstOrDefault(e => e.Id == context.VideoId);
            if (video is null)
                throw new EntityNotFoundException("Media not found");

            var audio = list.FirstOrDefault(e => e.Id == context.AudioId);
            if (audio is null)
                throw new EntityNotFoundException("Media not found");

            // check available space
            await _fileService.ThrowIfDoesNotHasAvailableFreeSpace(ct, video, audio);

            if (video.IsSkipped && audio.IsSkipped)
            {
                await telegramService.SendMessageAsync(context.ChatId, context.MessageId, "Nothing to download", ct);
            }
            else
            {
                await telegramService.SendMessageAsync(context.ChatId, context.MessageId, "Processing...", ct);

                using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
                tokenSource.CancelAfter(TimeSpan.FromHours(2));

                var fileId = await downloadingClient.DownloadAsync(context, video, audio, tokenSource.Token);

                var linkGenerator = scope.ServiceProvider.GetRequiredService<ICustomLinkGenerator>();
                var link = linkGenerator.GenerateFileLink(fileId, context.RequestContext);
                await telegramService.SendMessageAsync(context.ChatId, context.MessageId, link, tokenSource.Token);
            }
        }
        catch (NotAvailableSpaceException e)
        {
            _logger.LogError(e, "Download service failed");

            await sessionService.SetFailedSessionAsync(context.Id, e.ToString(), ct);

            await telegramService.SendMessageAsync(context.ChatId, context.MessageId, "Service is too busy. Please, try again later", ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Download service failed");

            await sessionService.SetFailedSessionAsync(context.Id, e.ToString(), ct);

            await telegramService.SendMessageAsync(context.ChatId, context.MessageId, "An error occured during processing", ct);
        }

        await sessionService.CompleteSessionAsync(context.Id, ct);

        _logger.LogInformation("WorkerInstance finished work: {WorkerId}", _workerId);

        // Pushing the instance back to the list of the free workers
        await _workersQueueService.QueueAsync(this, ct);
    }

    /// <inheritdoc />
    public int WorkerId => _workerId;

    #endregion
}