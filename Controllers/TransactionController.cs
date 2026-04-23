using System.Security.Claims;
using LedgerApi.Models;
using LedgerApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LedgerApi.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
public class TransactionController(ITransactionService txService) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public record CreateRequest(decimal Amount, TransactionKind Kind,
        string Category, string? Note, DateTime? Date);

    [HttpPost]
    public async Task<IActionResult> Create(CreateRequest req)
    {
        if (req.Amount <= 0) return BadRequest(new { error = "amount must be positive" });
        var tx = await txService.AddAsync(UserId, req.Amount, req.Kind,
            req.Category, req.Note, req.Date);
        return Created($"/api/transactions/{tx.Id}", tx);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int? month, [FromQuery] int? year) =>
        Ok(await txService.GetByUserAsync(UserId, month, year));

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(
        [FromQuery] int month = 0, [FromQuery] int year = 0)
    {
        if (month == 0) month = DateTime.UtcNow.Month;
        if (year == 0)  year  = DateTime.UtcNow.Year;
        return Ok(await txService.GetSummaryAsync(UserId, month, year));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await txService.DeleteAsync(UserId, id);
        return deleted ? NoContent() : NotFound();
    }
}
