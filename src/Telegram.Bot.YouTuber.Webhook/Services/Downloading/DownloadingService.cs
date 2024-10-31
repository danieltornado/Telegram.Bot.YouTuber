using Microsoft.EntityFrameworkCore;
using Telegram.Bot.YouTuber.Webhook.DataAccess;
using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;
using Telegram.Bot.YouTuber.Webhook.Extensions;
using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

internal sealed class DownloadingService : IDownloadingService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DownloadingService> _logger;

    public DownloadingService(AppDbContext dbContext, ILogger<DownloadingService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    #region Implementation of IDownloadingService

    public async Task<Guid> StartDownloadingAsync(SessionContext sessionContext, SessionMediaContext video, SessionMediaContext audio, CancellationToken ct)
    {
        DownloadingEntity entity = new()
        {
            CreatedAt = DateTime.UtcNow,
            SessionId = sessionContext.Id,

            VideoTitle = video.Title,
            VideoExtension = video.Extension,
            VideoUrl = video.InternalUrl,
            VideoQuality = video.Quality,
            VideoFormat = video.Format,

            AudioTitle = audio.Title,
            AudioExtension = audio.Extension,
            AudioUrl = audio.InternalUrl,
            AudioQuality = audio.Quality,
            AudioFormat = audio.Format
        };

        await _dbContext.Downloading.AddAsync(entity, ct);
        await _dbContext.SaveChangesAsync(ct);

        return entity.Id;
    }

    public async Task CompleteDownloadingAsync(DownloadingContext context, CancellationToken ct)
    {
        // Do not set Failed state because it does not matter

        try
        {
            var entity = await _dbContext.Downloading.FirstOrDefaultAsync(e => e.Id == context.Id, ct);
            if (entity is null)
            {
                _logger.LogError("The downloading entity was not found: {Id}", context.Id);
            }
            else
            {
                entity.UpdatedAt = DateTime.UtcNow;
                entity.IsCompleted = true;
                entity.Error = context.Error?.ToString();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to complete session: {Id}", context.Id);
        }
    }

    public async Task<DownloadingContext> GetDownloadingAsync(Guid downloadingId, CancellationToken ct)
    {
        DownloadingContext context = new();
        context.IsSuccess = true;
        context.Id = downloadingId;

        try
        {
            var entity = await _dbContext.Downloading.FirstOrDefaultAsync(e => e.Id == downloadingId, ct);
            if (entity != null)
            {
                context.Title = entity.GetTitle();
                context.Extension = entity.GetExtension();

                context.VideoQuality = entity.VideoQuality;
                context.VideoFormat = entity.VideoFormat;

                context.AudioQuality = entity.AudioQuality;
                context.AudioFormat = entity.AudioFormat;
            }
            else
            {
                context.IsSuccess = false;
                context.Error = new Exception("The entity was not found");
            }
        }
        catch (Exception e)
        {
            context.IsSuccess = false;
            context.Error = e;
        }

        return context;
    }

    #endregion

    
}