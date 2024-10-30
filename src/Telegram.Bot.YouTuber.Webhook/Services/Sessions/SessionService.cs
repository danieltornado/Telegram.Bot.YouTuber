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

        try
        {
            var entity = _dbContext.Sessions.Local.FirstOrDefault(e => e.Id == id);
            if (entity is null)
            {
                entity = await _dbContext.Sessions.Include(e => e.Media).FirstOrDefaultAsync(e => e.Id == id, ct);
            }

            if (entity != null)
            {
                context.Id = entity.Id;

                context.MessageId = entity.MessageId;
                context.ChatId = entity.ChatId;

                context.Json = entity.Json;
                context.JsonVideo = entity.JsonVideo;
                context.JsonAudio = entity.JsonAudio;

                context.Url = entity.Url;

                context.VideoId = entity.VideoId;
                context.AudioId = entity.AudioId;

                if (entity.Media != null)
                {
                    foreach (var media in entity.Media)
                    {
                        if (media.Type == MediaType.Video)
                        {
                            context.Videos.Add(new SessionMediaContext
                            {
                                Id = media.Id,
                                Extension = media.Extension,
                                Format = media.Format,
                                Quality = media.Quality,
                                InternalUrl = media.InternalUrl,
                                Title = media.Title,
                                Num = media.Num
                            });
                        }

                        if (media.Type == MediaType.Audio)
                        {
                            context.Audios.Add(new SessionMediaContext
                            {
                                Id = media.Id,
                                Extension = media.Extension,
                                Format = media.Format,
                                Quality = media.Quality,
                                InternalUrl = media.InternalUrl,
                                Title = media.Title,
                                Num = media.Num
                            });
                        }
                    }
                }
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

    public async Task<string?> GetTitleAsync(Guid id, CancellationToken ct)
    {
        string? title = null;

        try
        {
            var query = from s in _dbContext.Sessions
                        join m in _dbContext.Media on s.Id equals m.SessionId
                        where s.Id == id
                        where s.VideoId == m.Id
                        select new { m.Title, m.Extension };

            var data = await query.FirstOrDefaultAsync(ct);

            if (data is not null)
                title = data.Title + "." + data.Extension;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get title");
        }

        return title;
    }

    #endregion

    private async Task SaveSessionContextAsync(SessionContext sessionContext, bool isCompleted, CancellationToken ct)
    {
        try
        {
            var entity = _dbContext.Sessions.Local.FirstOrDefault(e => e.Id == sessionContext.Id);
            if (entity is null)
            {
                entity = await _dbContext.Sessions.Include(e => e.Media).FirstOrDefaultAsync(e => e.Id == sessionContext.Id, ct);
            }

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

                await MergeMedia(sessionContext.Videos, MediaType.Video, entity);
                await MergeMedia(sessionContext.Audios, MediaType.Audio, entity);

                _dbContext.Update(entity);
                await _dbContext.SaveChangesAsync(ct);
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

    private async Task MergeMedia(IReadOnlyList<SessionMediaContext> source, MediaType type, SessionEntity entity)
    {
        if (entity.Media is null)
        {
            entity.Media = new List<MediaEntity>();
        }

        foreach (var mediaContext in source)
        {
            var existMedia = entity.Media.FirstOrDefault(e => e.Id == mediaContext.Id);
            if (existMedia is null)
            {
                existMedia = new MediaEntity
                {
                    Extension = mediaContext.Extension,
                    Format = mediaContext.Format,
                    Quality = mediaContext.Quality,
                    InternalUrl = mediaContext.InternalUrl,
                    Id = mediaContext.Id,
                    Title = mediaContext.Title,
                    SessionId = mediaContext.Id,
                    Session = entity,
                    Type = type,
                    Num = mediaContext.Num
                };
                
                entity.Media.Add(existMedia);
                await _dbContext.Media.AddAsync(existMedia);
            }
            else
            {
                existMedia.Extension = mediaContext.Extension;
                existMedia.Format = mediaContext.Format;
                existMedia.Quality = mediaContext.Quality;
                existMedia.InternalUrl = mediaContext.InternalUrl;
                existMedia.Id = mediaContext.Id;
                existMedia.Title = mediaContext.Title;
                existMedia.SessionId = mediaContext.Id;
                existMedia.Session = entity;
                existMedia.Type = type;
                existMedia.Num = mediaContext.Num;

                _dbContext.Media.Update(existMedia);
            }
        }
    }
}