using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Sms.MockProvider.Models;

namespace Sms.MockProvider.Controllers;

[ApiController]
[Route("sms")]
public class SmsController : ControllerBase
{
    private static readonly ConcurrentDictionary<Guid, DateTimeOffset>
        Messages = new();

    [HttpPost]
    public IActionResult Send([FromBody] SendSmsRequest request)
    {
        var id = Guid.NewGuid();

        Messages[id] = DateTimeOffset.UtcNow;

        return CreatedAtAction(
            nameof(GetStatus),
            new { id },
            new { id, status = "accepted" });
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetStatus(Guid id)
    {
        if (!Messages.TryGetValue(id, out var acceptedAt))
        {
            return NotFound();
        }

        var elapsed = DateTimeOffset.UtcNow - acceptedAt;

        var status = elapsed.TotalSeconds >= 2
            ? "delivered"
            : "accepted";

        return Ok(new { id, status });
    }

}