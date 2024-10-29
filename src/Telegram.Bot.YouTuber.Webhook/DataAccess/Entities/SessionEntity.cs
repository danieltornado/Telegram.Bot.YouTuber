using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;

public sealed class SessionEntity
{
    [Key]
    public Guid Id { get; set; }

    public long? ChatId { get; set; }
    public int? MessageId { get; set; }
    public DateTime CreatedAt { get; set; }

    [MaxLength(2048)]
    public string? Url { get; set; }
    
    [MaxLength(32)]
    public string? VideoQuality { get; set; }
    
    [MaxLength(2048)]
    public string? VideoUrl { get; set; }
    
    [MaxLength(32)]
    public string? AudioQuality { get; set; }
    
    [MaxLength(2048)]
    public string? AudioUrl { get; set; }

    [SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
    public string? Json { get; set; }

    [SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
    public string? JsonVideo { get; set; }
    
    [SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
    public string? JsonAudio { get; set; }

    [MaxLength(1024)]
    public string? Title { get; set; }

    [MaxLength(8)]
    public string? Extension { get; set; }

    [SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
    public string? Error { get; set; }

    public DateTime UpdatedAt { get; set; }
    
    public bool IsCompleted { get; set; }
}