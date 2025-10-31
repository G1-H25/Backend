using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;


/// <summary>
/// Transport route management controller using dependency inversion principle.
/// </summary>
/// <remarks>
/// Manages logistics transport routes and delivery paths.
/// Demonstrates SOLID: SRP (route management), OCP (interface-based), LSP (substitutable),
/// ISP (focused interfaces), DIP (abstraction over concretions).
/// Constructor injection enables testing and loose coupling.
/// </remarks>
[ApiController]
[Route("[controller]")]
public class TransportRouteController : ControllerBase
{
    private readonly ISqlInsert _insertService;

    /// <summary>
    /// Initializes transport route controller with injected dependencies.
    /// </summary>
    /// <param name="insertService">Database insertion service (ISqlInsert).</param>
    /// <remarks>Constructor injection - dependencies provided by DI container for testability.</remarks>
    public TransportRouteController(ISqlInsert insertService)
    {
        _insertService = insertService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateTransportRoute([FromBody] TransportRouteCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Area))
        {
            return BadRequest("Code and Area are required.");
        }

        var values = new Dictionary<string, object>
        {
            { "Code", request.Code },
            { "Area", request.Area }
        };

        int newRouteId = await _insertService.InsertAndReturnIdAsync("Logistics.TransportRoute", values);

        return Ok(new { message = "Transport route created", routeId = newRouteId });
    }
}
