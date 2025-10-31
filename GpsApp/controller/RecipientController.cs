using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;

/// <summary>
/// Recipient management controller using dependency inversion principle.
/// </summary>
/// <remarks>
/// Manages logistics recipient entities and operations.
/// Demonstrates SOLID: SRP (recipient management), OCP (interface-based), LSP (substitutable),
/// ISP (focused interfaces), DIP (abstraction over concretions).
/// Constructor injection enables testing and loose coupling.
/// </remarks>
[ApiController]
[Route("[controller]")]
public class RecipientController : ControllerBase
{
    private readonly ISqlInsert _insertService;

    /// <summary>
    /// Initializes recipient controller with injected dependencies.
    /// </summary>
    /// <param name="insertService">Database insertion service (ISqlInsert).</param>
    /// <remarks>Constructor injection - dependencies provided by DI container for testability.</remarks>
    public RecipientController(ISqlInsert insertService)
    {
        _insertService = insertService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateRecipient([FromBody] RecipientCreateRequest request)
    {
        if (request.CompanyId <= 0)
            return BadRequest("Invalid CompanyId.");

        var values = new Dictionary<string, object>
        {
            { "CompanyId", request.CompanyId }
        };

        int id = await _insertService.InsertAndReturnIdAsync("Logistics.Recipient", values);

        return Ok(new { message = "Recipient created", id });
    }
}
