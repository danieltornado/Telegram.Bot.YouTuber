using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telegram.Bot.YouTuber.Webhook.DataAccess.Entities;

public sealed class MediaEntity
{
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
    
    [MaxLength(1024)]
    public string? Title { get; set; }

    [MaxLength(2048)]
    public required string InternalUrl { get; set; }

    public MediaType Type { get; set; }

    [MaxLength(32)]
    public required string Quality { get; set; }

    [MaxLength(16)]
    public required string Format { get; set; }

    [MaxLength(8)]
    public required string Extension { get; set; }
}