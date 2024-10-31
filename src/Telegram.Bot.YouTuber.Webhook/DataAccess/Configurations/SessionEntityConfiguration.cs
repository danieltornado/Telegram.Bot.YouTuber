using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;

namespace Telegram.Bot.YouTuber.Webhook.DataAccess.Configurations;

public sealed class SessionEntityConfiguration : IEntityTypeConfiguration<SessionEntity>
{
    #region Implementation of IEntityTypeConfiguration<SessionEntity>

    public void Configure(EntityTypeBuilder<SessionEntity> builder)
    {
        builder
            .Property(e => e.Json)
            .HasColumnType("jsonb");
        
        builder
            .Property(e => e.JsonVideo)
            .HasColumnType("jsonb");
        
        builder
            .Property(e => e.JsonAudio)
            .HasColumnType("jsonb");

        builder
            .HasMany(e => e.Media)
            .WithOne(e => e.Session)
            .HasForeignKey(e => e.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(e => e.Downloading)
            .WithOne(e => e.Session)
            .HasForeignKey(e => e.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    #endregion
}