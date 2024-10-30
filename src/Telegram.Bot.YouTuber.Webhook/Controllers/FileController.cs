using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.YouTuber.Webhook.Extensions;
using Telegram.Bot.YouTuber.Webhook.Services.Files;
using Telegram.Bot.YouTuber.Webhook.Services.Sessions;

namespace Telegram.Bot.YouTuber.Webhook.Controllers;

[ApiController]
[Route("api/files")]
public sealed class FileController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ISessionService _sessionService;
    
    public FileController(IFileService fileService, ISessionService sessionService)
    {
        _fileService = fileService;
        _sessionService = sessionService;
    }

    [HttpGet("{fileId}")]
    public async Task<IActionResult> GetFile(string fileId, CancellationToken ct)
    {
        var sessionId = fileId.ToGuid();
        if (sessionId is null)
            return BadRequest();

        var title = await _sessionService.GetTitleAsync(sessionId.Value, ct);

        var fileStream = _fileService.OpenFinalFile(sessionId.Value);
        if (fileStream is null)
            return NotFound();

        return File(fileStream, "application/octet-stream", title);
    }
}