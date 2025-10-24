using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;

/// <summary>
/// Sender management controller using dependency inversion principle.
/// </summary>
/// <remarks>
/// Manages logistics sender entities and operations.
/// Demonstrates SOLID: SRP (sender management), OCP (interface-based), LSP (substitutable),
/// ISP (focused interfaces), DIP (abstraction over concretions).
/// Constructor injection enables testing and loose coupling.
/// </remarks>
[ApiController]
[Route("[controller]")]
public class SenderController : ControllerBase
{
    private readonly ISqlInsert _insertService;

    /// <summary>
    /// Initializes sender controller with injected dependencies.
    /// </summary>
    /// <param name="insertService">Database insertion service (ISqlInsert).</param>
    /// <remarks>Constructor injection - dependencies provided by DI container for testability.</remarks>
    public SenderController(ISqlInsert insertService)
    {
        _insertService = insertService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateSender([FromBody] SenderCreateRequest request)
    {
        if (request.CompanyId <= 0)
            return BadRequest("Invalid CompanyId.");

        var values = new Dictionary<string, object>
        {
            { "CompanyId", request.CompanyId }
        };

        int id = await _insertService.InsertAndReturnIdAsync("Logistics.Sender", values);

        return Ok(new { message = "Sender created", id });
    }
}
