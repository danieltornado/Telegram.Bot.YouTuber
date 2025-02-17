using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.YouTuber.Webhook.BL.Abstractions;
using Telegram.Bot.YouTuber.Webhook.Extensions;

namespace Telegram.Bot.YouTuber.Webhook.Controllers;

[ApiController]
[Route("api/test")]
public sealed class TestController : ControllerBase
{
    /// <summary>
    /// Tests custom exception handling
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    [HttpGet("exception")]
    public IActionResult TestException()
    {
        throw new NotSupportedException("If you see this message, the environment is not Production");
    }
    
    /// <summary>
    /// Tests link generation
    /// </summary>
    /// <param name="linkGenerator"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    [HttpGet("link")]
    public IActionResult GetLink([FromServices] ICustomLinkGenerator linkGenerator, [FromServices] IConfiguration configuration)
    {
        return Ok(linkGenerator.GenerateFileLink(fileId: Guid.NewGuid(), requestContext: new RequestContext
        {
            Host = Request.Host,
            PathFile = configuration.GetPathFile(),
            Scheme = Request.Scheme
        }));
    }
}