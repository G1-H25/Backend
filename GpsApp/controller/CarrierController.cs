using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;

/// <summary>
/// Carrier and vehicle management controller using dependency inversion principle.
/// </summary>
/// <remarks>
/// Manages complete carrier registration with multi-step entity creation.
/// Demonstrates SOLID: SRP (carrier management), OCP (interface-based), LSP (substitutable),
/// ISP (focused interfaces), DIP (abstraction over concretions).
/// Creates Registration → Vehicle → Carrier relationship chain.
/// Constructor injection enables testing and loose coupling.
/// </remarks>
[ApiController]
[Route("[controller]")]
public class CarrierController : ControllerBase
{
    private readonly ISqlInsert _insertService;

    /// <summary>
    /// Initializes carrier controller with injected dependencies.
    /// </summary>
    /// <param name="insertService">Database insertion service (ISqlInsert).</param>
    /// <remarks>Constructor injection - dependencies provided by DI container for testability. Handles multi-step entity creation.</remarks>
    public CarrierController(ISqlInsert insertService)
    {
        _insertService = insertService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateCarrier([FromBody] CarrierCreateRequest request)
    {
        if (request.CompanyId <= 0 || request.GatewayId <= 0)
            return BadRequest("Invalid CompanyId or GatewayId.");

        if (string.IsNullOrWhiteSpace(request.Plate) ||
            string.IsNullOrWhiteSpace(request.Brand) ||
            string.IsNullOrWhiteSpace(request.Model))
        {
            return BadRequest("Registration info is incomplete.");
        }

        // Step 1: Insert Registration
        var registrationValues = new Dictionary<string, object>
        {
            { "Plate", request.Plate },
            { "Brand", request.Brand },
            { "Model", request.Model }
        };

        int registrationId = await _insertService.InsertAndReturnIdAsync("Secrets.Registration", registrationValues);

        // Step 2: Insert Vehicle
        var vehicleValues = new Dictionary<string, object>
        {
            { "GatewayId", request.GatewayId },
            { "RegistrationId", registrationId }
        };

        int vehicleId = await _insertService.InsertAndReturnIdAsync("Secrets.Vehicle", vehicleValues);

        // Step 3: Insert Carrier
        var carrierValues = new Dictionary<string, object>
        {
            { "CompanyId", request.CompanyId },
            { "VehicleId", vehicleId }
        };

        int carrierId = await _insertService.InsertAndReturnIdAsync("Logistics.Carrier", carrierValues);

        return Ok(new { message = "Carrier created", carrierId });
    }
}
