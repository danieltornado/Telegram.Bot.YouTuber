using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.YouTuber.Webhook.Extensions;
using Telegram.Bot.YouTuber.Webhook.Services.Downloading;
using Telegram.Bot.YouTuber.Webhook.Services.Files;
using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Controllers;

[ApiController]
[Route("api/files")]
public sealed class FileController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly IDownloadingService _downloadingService;
    private readonly ILogger<FileController> _logger;

    public FileController(IFileService fileService, IDownloadingService downloadingService, ILogger<FileController> logger)
    {
        _fileService = fileService;
        _downloadingService = downloadingService;
        _logger = logger;
    }

    [HttpGet("{fileId:guid}")]
    public async Task<IActionResult> GetFile(Guid fileId, CancellationToken ct)
    {
        var downloadingContext = await _downloadingService.GetDownloadingAsync(fileId, ct);
        if (!downloadingContext.IsSuccess)
        {
            _logger.LogError(downloadingContext.Error, "Failed to get a file: {Id}", fileId);
            return BadRequest();
        }

        string title = downloadingContext.GetTitleWithExtension();

        var fileStream = _fileService.OpenFinalFile(downloadingContext.Id);
        if (fileStream is null)
            return NotFound();

        return File(fileStream, "application/octet-stream", title);
    }
}