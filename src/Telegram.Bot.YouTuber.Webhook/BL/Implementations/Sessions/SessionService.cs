using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions.Sessions;
using Telegram.Bot.YouTuber.Webhook.DataAccess;
using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;
using Telegram.Bot.YouTuber.Webhook.DataAccess.Exceptions;

namespace Telegram.Bot.YouTuber.Webhook.BL.Implementations.Sessions;

/// <inheritdoc cref="ISessionService" />
internal sealed class SessionService : ISessionService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ILogger<SessionService> _logger;

    public SessionService(AppDbContext dbContext, IMapper mapper, ILogger<SessionService> logger)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _logger = logger;
    }

    #region Implementation of ISessionService

    /// <inheritdoc />
    public async Task<SessionContext> StartSessionAsync(StartSessionContext context, CancellationToken ct)
    {
        var entity = _mapper.Map<SessionEntity>(context);
        entity.CreatedAt = DateTime.UtcNow;

        await _dbContext.Sessions.AddAsync(entity, ct);
        await _dbContext.SaveChangesAsync(ct);

        return _mapper.Map<SessionContext>(entity);
    }

    /// <inheritdoc />
    public async Task<List<SessionMediaContext>> SaveMediaAsync(Guid sessionId, IReadOnlyCollection<VideoInfo> video, IReadOnlyCollection<AudioInfo> audio, CancellationToken ct)
    {
        foreach (var videoInfo in video)
        {
            var mediaEntity = _mapper.Map<MediaEntity>(videoInfo);
            mediaEntity.SessionId = sessionId;
            await _dbContext.Media.AddAsync(mediaEntity, ct);
        }

        // SKIP video
        await _dbContext.Media.AddAsync(new MediaEntity
            {
                SessionId = sessionId,
                Type = MediaType.Video,
                IsSkipped = true
            },
            ct);

        foreach (var audioInfo in audio)
        {
            var mediaEntity = _mapper.Map<MediaEntity>(audioInfo);
            mediaEntity.SessionId = sessionId;
            await _dbContext.Media.AddAsync(mediaEntity, ct);
        }

        // SKIP audio
        await _dbContext.Media.AddAsync(new MediaEntity
            {
                SessionId = sessionId,
                Type = MediaType.Audio,
                IsSkipped = true
            },
            ct);

        await _dbContext.SaveChangesAsync(ct);

        List<SessionMediaContext> mediaList = new(_dbContext.Media.Local.Count);
        foreach (var mediaEntity in _dbContext.Media.Local)
        {
            var sessionMediaContext = _mapper.Map<SessionMediaContext>(mediaEntity);
            mediaList.Add(sessionMediaContext);
        }

        return mediaList;
    }

    /// <inheritdoc />
    public async Task CompleteSessionAsync(Guid sessionId, CancellationToken ct)
    {
        try
        {
            await _dbContext.Sessions
                .Where(e => e.Id == sessionId)
                .ExecuteUpdateAsync(e =>
                        e.SetProperty(s => s.UpdatedAt, DateTime.UtcNow).SetProperty(s => s.IsCompleted, true),
                    ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to complete session: {Id}", sessionId);
        }
    }

    /// <inheritdoc />
    public async Task SetFailedSessionAsync(Guid sessionId, string error, CancellationToken ct)
    {
        try
        {
            await _dbContext.Sessions
                .Where(e => e.Id == sessionId)
                .ExecuteUpdateAsync(e =>
                        e.SetProperty(s => s.UpdatedAt, DateTime.UtcNow).SetProperty(s => s.Error, error),
                    ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to complete session: {Id}", sessionId);
        }
    }

    /// <inheritdoc />
    public async Task<SessionContext> GetSessionByMediaAsync(Guid mediaId, CancellationToken ct)
    {
        var entity = await _dbContext.Sessions.AsNoTracking().Where(e => e.Media!.Any(s => s.Id == mediaId)).FirstOrDefaultAsync(ct);
        if (entity is null)
            throw new EntityNotFoundException("Session not found");

        return _mapper.Map<SessionContext>(entity);
    }

    /// <inheritdoc />
    public async Task<List<SessionMediaContext>> GetMediaBySessionAsync(Guid sessionId, MediaType mediaType, CancellationToken ct)
    {
        var entities = await _dbContext.Media.AsNoTracking().Where(e => e.SessionId == sessionId && e.Type == mediaType).ToListAsync(ct);
        return _mapper.Map<List<SessionMediaContext>>(entities);
    }

    /// <inheritdoc />
    public async Task SaveSessionAnswerAsync(Guid mediaId, string? json, CancellationToken ct)
    {
        var mediaEntity = await _dbContext.Media.Include(e => e.Session).FirstOrDefaultAsync(e => e.Id == mediaId, ct);
        if (mediaEntity is null)
            throw new EntityNotFoundException("File not found");

        if (mediaEntity.Type == MediaType.Video)
        {
            mediaEntity.Session!.VideoId = mediaId;
            mediaEntity.Session!.JsonVideo = json;
            mediaEntity.Session!.UpdatedAt = DateTime.UtcNow;
        }
        else if (mediaEntity.Type == MediaType.Audio)
        {
            mediaEntity.Session!.AudioId = mediaId;
            mediaEntity.Session!.JsonAudio = json;
            mediaEntity.Session!.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<List<SessionMediaContext>> GetMediaAsync(IEnumerable<Guid> mediaIds, CancellationToken ct)
    {
        var entities = await _dbContext.Media.AsNoTracking().Where(e => mediaIds.Contains(e.Id)).ToListAsync(ct);
        return _mapper.Map<List<SessionMediaContext>>(entities);
    }

    #endregion
}