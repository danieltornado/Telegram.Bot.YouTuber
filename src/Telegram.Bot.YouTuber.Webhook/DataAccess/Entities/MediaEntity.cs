using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;

public sealed class MediaEntity
{
    [Key]
    public Guid Id { get; set; }

    #region SessionEntity

    public Guid SessionId { get; set; }

    [ForeignKey(nameof(SessionId))]
    public SessionEntity? Session { get; set; }

    #endregion

    /// <summary>
    /// Unique number for a media
    /// </summary>
    /// <remarks>Eliminates the need to use an identifier</remarks>
    public int Num { get; set; }
    
    [MaxLength(AppDbConstants.TITLE_LENGTH)]
    public string? Title { get; set; }

    [MaxLength(AppDbConstants.URL_LENGTH)]
    public string? InternalUrl { get; set; }

    public MediaType Type { get; set; }

    [MaxLength(AppDbConstants.QUALITY_LENGTH)]
    public string? Quality { get; set; }

    [MaxLength(AppDbConstants.FORMAT_LENGTH)]
    public string? Format { get; set; }

    [MaxLength(AppDbConstants.EXTENSION_LENGTH)]
    public string? Extension { get; set; }
    
    public long? ContentLength { get; set; }
    
    public bool IsSkipped { get; set; }
}