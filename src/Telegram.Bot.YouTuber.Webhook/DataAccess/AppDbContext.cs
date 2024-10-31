using Microsoft.EntityFrameworkCore;
using Telegram.Bot.YouTuber.Webhook.DataAccess.Configurations;
using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;

namespace Telegram.Bot.YouTuber.Webhook.DataAccess;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        
    }
    
    public DbSet<SessionEntity> Sessions { get; private set; } = null!;
    
    public DbSet<MediaEntity> Media { get; private set; } = null!;
    
    public DbSet<DownloadingEntity> Downloading { get; private set; } = null!;

    #region Overrides of DbContext

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SessionEntityConfiguration).Assembly);
    }

    #endregion
}