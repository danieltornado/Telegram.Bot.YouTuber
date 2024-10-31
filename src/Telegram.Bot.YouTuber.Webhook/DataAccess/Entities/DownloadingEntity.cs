using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;

public sealed class DownloadingEntity
{
    [Key]
    public Guid Id { get; set; }

    #region SessionEntity

    public Guid SessionId { get; set; }

    [ForeignKey(nameof(SessionId))]
    public SessionEntity? Session { get; set; }

    #endregion

    [MaxLength(AppDbConstants.TITLE_LENGTH)]
    public string? VideoTitle { get; set; }

    [MaxLength(AppDbConstants.URL_LENGTH)]
    public string? VideoUrl { get; set; }

    [MaxLength(AppDbConstants.QUALITY_LENGTH)]
    public string? VideoQuality { get; set; }
    
    [MaxLength(AppDbConstants.FORMAT_LENGTH)]
    public string? VideoFormat { get; set; }

    [MaxLength(AppDbConstants.EXTENSION_LENGTH)]
    public string? VideoExtension { get; set; }
    
    [MaxLength(AppDbConstants.TITLE_LENGTH)]
    public string? AudioTitle { get; set; }

    [MaxLength(AppDbConstants.URL_LENGTH)]
    public string? AudioUrl { get; set; }

    [MaxLength(AppDbConstants.QUALITY_LENGTH)]
    public string? AudioQuality { get; set; }
    
    [MaxLength(AppDbConstants.FORMAT_LENGTH)]
    public string? AudioFormat { get; set; }

    [MaxLength(AppDbConstants.EXTENSION_LENGTH)]
    public string? AudioExtension { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    [SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
    public string? Error { get; set; }

    public bool IsCompleted { get; set; }
}