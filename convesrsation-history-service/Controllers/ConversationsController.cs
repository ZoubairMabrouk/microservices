using ConversationHistoryService.DTOs;
using ConversationHistoryService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConversationHistoryService.Controllers;


[ApiController]
[Route("api/conversations")]
[Authorize]
public class ConversationsController : ControllerBase
{
    private readonly IConversationService _service;

    public ConversationsController(IConversationService service) => _service = service;

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException());

    private bool IsAdmin() =>
        User.IsInRole("Admin");

    // ── GET /api/conversations ─────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        if (IsAdmin())
        {
            var all = await _service.GetAllConversationsAsync(ct);
            return Ok(all);
        }
        var mine = await _service.GetMyConversationsAsync(GetUserId(), ct);
        return Ok(mine);
    }

    // ── GET /api/conversations/{id} ────────────────────────────
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var conv = await _service.GetConversationAsync(id, GetUserId(), IsAdmin(), ct);
            return Ok(conv);
        }
        catch (KeyNotFoundException ex)         { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex)  { return Forbid(); }
    }

    // ── POST /api/conversations ────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConversationDto dto, CancellationToken ct)
    {
        var created = await _service.CreateConversationAsync(GetUserId(), dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // ── POST /api/conversations/{id}/messages ──────────────────
    [HttpPost("{id:guid}/messages")]
    public async Task<IActionResult> AddMessage(Guid id, [FromBody] AddMessageDto dto, CancellationToken ct)
    {
        try
        {
            var msg = await _service.AddMessageAsync(id, GetUserId(), IsAdmin(), dto, ct);
            return Ok(msg);
        }
        catch (KeyNotFoundException ex)         { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException)     { return Forbid(); }
        catch (ArgumentException ex)            { return BadRequest(new { message = ex.Message }); }
    }

    // ── DELETE /api/conversations/{id} ────────────────────────
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.DeleteConversationAsync(id, GetUserId(), IsAdmin(), ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)         { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException)     { return Forbid(); }
    }
}