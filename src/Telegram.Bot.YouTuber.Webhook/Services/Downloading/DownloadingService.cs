using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.YouTuber.Webhook.DataAccess;
using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;
using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Downloading;

internal sealed class DownloadingService : IDownloadingService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ILogger<DownloadingService> _logger;

    public DownloadingService(AppDbContext dbContext, IMapper mapper, ILogger<DownloadingService> logger)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _logger = logger;
    }

    #region Implementation of IDownloadingService

    public async Task<Guid> StartDownloadingAsync(Guid sessionId, SessionMediaContext video, SessionMediaContext audio, CancellationToken ct)
    {
        DownloadingEntity entity = new()
        {
            CreatedAt = DateTime.UtcNow,
            SessionId = sessionId,

            VideoTitle = video.Title,
            VideoExtension = video.Extension,
            VideoUrl = video.InternalUrl,
            VideoQuality = video.Quality,
            VideoFormat = video.Format,
            VideoContentLength = video.ContentLength,

            AudioTitle = audio.Title,
            AudioExtension = audio.Extension,
            AudioUrl = audio.InternalUrl,
            AudioQuality = audio.Quality,
            AudioFormat = audio.Format,
            AudioContentLength = audio.ContentLength
        };

        await _dbContext.Downloading.AddAsync(entity, ct);
        await _dbContext.SaveChangesAsync(ct);

        return entity.Id;
    }

    public async Task CompleteDownloadingAsync(Guid downloadingId, CancellationToken ct)
    {
        // Do not set Failed state because it does not matter

        try
        {
            await _dbContext.Downloading
                .Where(e => e.Id == downloadingId)
                .ExecuteUpdateAsync(e =>
                        e.SetProperty(s => s.UpdatedAt, DateTime.UtcNow).SetProperty(s => s.IsCompleted, true),
                    ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to complete downloading: {Id}", downloadingId);
        }
    }

    public async Task<DownloadingContext?> GetDownloadingAsync(Guid downloadingId, CancellationToken ct)
    {
        var entity = await _dbContext.Downloading.FirstOrDefaultAsync(e => e.Id == downloadingId, ct);
        if (entity != null)
        {
            return _mapper.Map<DownloadingContext>(entity);
        }

        return null;
    }

    public async Task SetFailedDownloadingAsync(Guid downloadingId, string error, CancellationToken ct)
    {
        try
        {
            await _dbContext.Downloading
                .Where(e => e.Id == downloadingId)
                .ExecuteUpdateAsync(e =>
                        e.SetProperty(s => s.UpdatedAt, DateTime.UtcNow).SetProperty(s => s.Error, error),
                    ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to complete downloading: {Id}", downloadingId);
        }
    }

    #endregion
}