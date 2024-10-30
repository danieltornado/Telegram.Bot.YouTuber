using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

    [SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
    public string? Json { get; set; }

    [SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
    public string? JsonVideo { get; set; }

    [SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
    public string? JsonAudio { get; set; }

    [SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
    public string? Error { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsCompleted { get; set; }

    public Guid? VideoId { get; set; }

    public Guid? AudioId { get; set; }

    public List<MediaEntity>? Media { get; set; }
}