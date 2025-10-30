using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;

/// <summary>
/// Expected temperature management controller using dependency inversion principle.
/// </summary>
/// <remarks>
/// Configures acceptable temperature ranges for sensor monitoring.
/// Demonstrates SOLID: SRP (temperature expectations), OCP (interface-based), LSP (substitutable),
/// ISP (focused interfaces), DIP (abstraction over concretions).
/// Constructor injection enables testing and loose coupling.
/// </remarks>
[ApiController]
[Route("[controller]")]
public class ExpectedTempController : ControllerBase
{
    private readonly ISqlInsert _insertService;
    private readonly ISqlUpdate _sqlUpdate;

    /// <summary>
    /// Initializes expected temperature controller with injected dependencies.
    /// </summary>
    /// <param name="insertService">Database insertion service (ISqlInsert).</param>
    /// <param name="sqlUpdate">Database update service (ISqlUpdate).</param>
    /// <remarks>Constructor injection - dependencies provided by DI container for testability. Creates temperature expectations and links to sensors.</remarks>
    public ExpectedTempController(ISqlInsert insertService, ISqlUpdate sqlUpdate)
    {
        _insertService = insertService;
        _sqlUpdate = sqlUpdate;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateExpectedTemp([FromBody] ExpectedTempCreateRequest request)
    {
        // Basic validation
        if (string.IsNullOrEmpty(request.Note) || request.Min >= request.Max)
        {
            return BadRequest("Invalid ExpectedTemp data.");
        }

        if (request.SensorId <= 0)
        {
            return BadRequest("Invalid SensorId.");
        }

        var values = new Dictionary<string, object>
        {
            { "Note", request.Note },
            { "Min", request.Min },
            { "Max", request.Max }
        };

        // 1. Insert new ExpectedTemp row and get its ID
        int expectedTempId = await _insertService.InsertAndReturnIdAsync("Measurements.ExpectedTemp", values);

        // 2. Update sensor to point to the new ExpectedTemp
        await _sqlUpdate.UpdateAsync("Measurements.Sensor",
            new Dictionary<string, object> { { "ExpectedTempId", expectedTempId } },
            new Dictionary<string, object> { { "Id", request.SensorId } });

        return Ok(new { message = "ExpectedTemp created and sensor updated", expectedTempId });
    }
}

