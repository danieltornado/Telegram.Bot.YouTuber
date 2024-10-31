using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Bot.YouTuber.Webhook.DataAccess;
using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;
using Telegram.Bot.YouTuber.Webhook.Extensions;

namespace Telegram.Bot.YouTuber.Webhook.Services.Sessions;

internal sealed class SessionService : ISessionService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public SessionService(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
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
                _mapper.Map(entity, context);

                FillMediaToContext(context.Videos, entity, MediaType.Video);
                FillMediaToContext(context.Audios, entity, MediaType.Audio);
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
                _mapper.Map(sessionContext, entity);

                if (isCompleted)
                    entity.IsCompleted = true;

                entity.UpdatedAt = DateTime.UtcNow;
                entity.Error = sessionContext.Error?.ToString();

                await AddNewMediaToEntity(sessionContext.Videos, MediaType.Video, entity);
                await AddNewMediaToEntity(sessionContext.Audios, MediaType.Audio, entity);

                _dbContext.Update(entity);
                await _dbContext.SaveChangesAsync(ct);

                sessionContext.Videos.Clear();
                sessionContext.Audios.Clear();

                // MediaEntity has already got an Id
                FillMediaToContext(sessionContext.Videos, entity, MediaType.Video);
                FillMediaToContext(sessionContext.Audios, entity, MediaType.Audio);
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

    private async Task AddNewMediaToEntity(IReadOnlyList<SessionMediaContext> source, MediaType type, SessionEntity sessionEntity)
    {
        if (sessionEntity.Media is null)
        {
            sessionEntity.Media = new List<MediaEntity>();
        }

        foreach (var mediaContext in source)
        {
            if (mediaContext.Id == Guid.Empty)
            {
                var newMedia = _mapper.Map<MediaEntity>(mediaContext);
                newMedia.Type = type;

                sessionEntity.Media.Add(newMedia);
                await _dbContext.Media.AddAsync(newMedia);
            }
        }
    }

    private void FillMediaToContext(ICollection<SessionMediaContext> target, SessionEntity sessionEntity, MediaType type)
    {
        if (sessionEntity.Media != null)
        {
            foreach (var mediaEntity in sessionEntity.Media)
            {
                if (mediaEntity.Type == type)
                {
                    var sessionMediaContext = _mapper.Map<SessionMediaContext>(mediaEntity);
                    target.Add(sessionMediaContext);
                }
            }
        }
    }
}