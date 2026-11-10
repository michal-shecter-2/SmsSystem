using Microsoft.AspNetCore.Mvc;
using Sms.Api.Models;
using Sms.Api.Services;

namespace Sms.Api.Controllers;

[ApiController]
[Route("messages")]
public class MessagesController : ControllerBase
{
    private readonly MessageService _service;

    public MessagesController(MessageService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] SendMessageRequest request)
    {
        var result = await _service.SendAsync(request);

        return StatusCode(StatusCodes.Status201Created, new
        {
            id = result.Id,
            status = result.Status.ToString().ToLowerInvariant()
        });
    }


    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var message = await _service.GetAsync(id);

        if (message is null)
        {
            return NotFound();
        }

        return Ok(message);
    }
    [HttpGet]
    public async Task<IActionResult> List(
    [FromQuery] MessageStatus? status = null)
    {
        if (status.HasValue &&
            !Enum.IsDefined(typeof(MessageStatus), status.Value))
        {
            return BadRequest("Invalid status.");
        }

        var messages = await _service.ListAsync(status);

        return Ok(messages);
    }
}