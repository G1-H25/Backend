using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;

/// <summary>
/// Delivery state management controller using dependency inversion principle.
/// </summary>
/// <remarks>
/// Tracks delivery lifecycle states and status transitions.
/// Demonstrates SOLID: SRP (state management), OCP (interface-based), LSP (substitutable),
/// ISP (focused interfaces), DIP (abstraction over concretions).
/// Constructor injection enables testing and loose coupling.
/// </remarks>
[ApiController]
[Route("[controller]")]
public class DeliveryStateController : ControllerBase
{
    private readonly ISqlInsert _insertService;

    /// <summary>
    /// Initializes delivery state controller with injected dependencies.
    /// </summary>
    /// <param name="insertService">Database insertion service (ISqlInsert).</param>
    /// <remarks>Constructor injection - dependencies provided by DI container for testability.</remarks>
    public DeliveryStateController(ISqlInsert insertService)
    {
        _insertService = insertService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateDeliveryState([FromBody] DeliveryStateCreateRequest request)
    {
        if (request.DeliveryId <= 0)
            return BadRequest("Invalid DeliveryId.");

        if (string.IsNullOrWhiteSpace(request.CurrentState))
            return BadRequest("CurrentState is required.");

        var values = new Dictionary<string, object>
        {
            { "DeliveryId", request.DeliveryId },
            { "CurrentState", request.CurrentState },
            { "UpdatedAt", request.UpdatedAt }
        };

        int id = await _insertService.InsertAndReturnIdAsync("Orders.DeliveryState", values);

        return Ok(new { message = "Delivery state created", id });
    }
}
