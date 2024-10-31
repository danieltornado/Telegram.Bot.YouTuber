using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Bot.YouTuber.Webhook.DataAccess;
using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;
using Telegram.Bot.YouTuber.Webhook.Extensions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Sessions;

internal sealed class SessionService : ISessionService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<SessionService> _logger;

    public SessionService(AppDbContext dbContext, ILogger<SessionService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    #region Implementation of ISessionService

    public async Task<SessionContext> StartSessionAsync(Update update, CancellationToken ct)
    {
        SessionContext context = new();
        context.IsSuccess = true;

        try
        {
            context.MessageId = update.Message?.MessageId;
            context.ChatId = update.Message?.Chat.Id;
            context.Json = update.CreateJson();
            context.Url = update.Message?.Text;

            SessionEntity entity = new()
            {
                Json = context.Json,
                CreatedAt = DateTime.UtcNow,
                MessageId = context.MessageId,
                ChatId = context.ChatId,
                Url = context.Url
            };

            await _dbContext.Sessions.AddAsync(entity, ct);
            await _dbContext.SaveChangesAsync(ct);

            context.Id = entity.Id;
        }
        catch (Exception e)
        {
            context.IsSuccess = false;
            context.Error = e;
        }

        return context;
    }

    public async Task<SessionContext> ReadSessionAsync(Guid id, Update update, CancellationToken ct)
    {
        SessionContext context = new();
        context.IsSuccess = true;
        context.Id = id;

        try
        {
            var entity = await _dbContext.Sessions.Include(e => e.Media).FirstOrDefaultAsync(e => e.Id == id, ct);
            if (entity != null)
            {
                context.MessageId = entity.MessageId;
                context.ChatId = entity.ChatId;

                context.Json = entity.Json;
                context.JsonVideo = entity.JsonVideo;
                context.JsonAudio = entity.JsonAudio;

                context.Url = entity.Url;

                context.VideoId = entity.VideoId;
                context.AudioId = entity.AudioId;

                FillMedia(context.Videos, entity, MediaType.Video);
                FillMedia(context.Audios, entity, MediaType.Audio);
            }
            else
            {
                context.IsSuccess = false;
                context.Error = new Exception("Not found");
            }
        }
        catch (Exception e)
        {
            context.IsSuccess = false;
            context.Error = e;
        }

        return context;
    }

    public Task SaveSessionAsync(SessionContext sessionContext, CancellationToken ct)
    {
        return SaveSessionContextAsync(sessionContext, false, ct);
    }

    public Task CompleteSessionAsync(SessionContext sessionContext, CancellationToken ct)
    {
        return SaveSessionContextAsync(sessionContext, true, ct);
    }

    #endregion

    private async Task SaveSessionContextAsync(SessionContext sessionContext, bool isCompleted, CancellationToken ct)
    {
        try
        {
            var entity = await _dbContext.Sessions.Include(e => e.Media).FirstOrDefaultAsync(e => e.Id == sessionContext.Id, ct);

            if (entity is not null)
            {
                if (isCompleted)
                    entity.IsCompleted = true;

                entity.Json = sessionContext.Json;
                entity.JsonVideo = sessionContext.JsonVideo;
                entity.JsonAudio = sessionContext.JsonAudio;

                entity.Url = sessionContext.Url;

                entity.VideoId = sessionContext.VideoId;
                entity.AudioId = sessionContext.AudioId;

                entity.UpdatedAt = DateTime.UtcNow;
                entity.Error = sessionContext.Error?.ToString();

                await AddNewMedia(sessionContext.Videos, MediaType.Video, entity);
                await AddNewMedia(sessionContext.Audios, MediaType.Audio, entity);

                _dbContext.Update(entity);
                await _dbContext.SaveChangesAsync(ct);

                sessionContext.Videos.Clear();
                sessionContext.Audios.Clear();

                FillMedia(sessionContext.Videos, entity, MediaType.Video);
                FillMedia(sessionContext.Audios, entity, MediaType.Audio);
            }
            else
            {
                sessionContext.IsSuccess = false;
                sessionContext.Error = new Exception("Not found");
            }
        }
        catch (Exception e)
        {
            sessionContext.IsSuccess = false;
            sessionContext.Error = e;
        }
    }

    private async Task AddNewMedia(IReadOnlyList<SessionMediaContext> source, MediaType type, SessionEntity sessionEntity)
    {
        if (sessionEntity.Media is null)
        {
            sessionEntity.Media = new List<MediaEntity>();
        }

        foreach (var mediaContext in source)
        {
            if (mediaContext.Id == Guid.Empty)
            {
                var newMedia = new MediaEntity
                {
                    Extension = mediaContext.Extension,
                    Format = mediaContext.Format,
                    Quality = mediaContext.Quality,
                    InternalUrl = mediaContext.InternalUrl,
                    Title = mediaContext.Title,
                    SessionId = sessionEntity.Id,
                    Type = type,
                    Num = mediaContext.Num,
                    IsSkipped = mediaContext.IsSkipped
                };

                sessionEntity.Media.Add(newMedia);
                await _dbContext.Media.AddAsync(newMedia);
            }
        }
    }

    private void FillMedia(ICollection<SessionMediaContext> target, SessionEntity sessionEntity, MediaType type)
    {
        if (sessionEntity.Media != null)
        {
            foreach (var mediaEntity in sessionEntity.Media)
            {
                if (mediaEntity.Type == type)
                {
                    target.Add(new SessionMediaContext
                    {
                        Id = mediaEntity.Id,
                        Extension = mediaEntity.Extension,
                        Format = mediaEntity.Format,
                        Quality = mediaEntity.Quality,
                        InternalUrl = mediaEntity.InternalUrl,
                        Title = mediaEntity.Title,
                        Num = mediaEntity.Num,
                        IsSkipped = mediaEntity.IsSkipped
                    });
                }
            }
        }
    }
}