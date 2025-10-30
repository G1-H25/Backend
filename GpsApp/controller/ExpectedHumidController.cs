using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;

/// <summary>
/// Expected humidity management controller using dependency inversion principle.
/// </summary>
/// <remarks>
/// Configures acceptable humidity ranges for sensor monitoring.
/// Demonstrates SOLID: SRP (humidity expectations), OCP (interface-based), LSP (substitutable),
/// ISP (focused interfaces), DIP (abstraction over concretions).
/// Constructor injection enables testing and loose coupling.
/// </remarks>
[ApiController]
[Route("[controller]")]
public class ExpectedHumidController : ControllerBase
{
    private readonly ISqlInsert _insertService;
    private readonly ISqlUpdate _sqlUpdate;

    /// <summary>
    /// Initializes expected humidity controller with injected dependencies.
    /// </summary>
    /// <param name="insertService">Database insertion service (ISqlInsert).</param>
    /// <param name="sqlUpdate">Database update service (ISqlUpdate).</param>
    /// <remarks>Constructor injection - dependencies provided by DI container for testability. Creates humidity expectations and links to sensors.</remarks>
    public ExpectedHumidController(ISqlInsert insertService, ISqlUpdate sqlUpdate)
    {
        _insertService = insertService;
        _sqlUpdate = sqlUpdate;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateExpectedHumid([FromBody] ExpectedHumidCreateRequest request)
    {
        // Basic validation
        if (string.IsNullOrEmpty(request.Note) || request.Min >= request.Max)
        {
            return BadRequest("Invalid ExpectedHumid data.");
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

        // 1. Insert new ExpectedHumid row and get its ID
        int expectedHumidId = await _insertService.InsertAndReturnIdAsync("Measurements.ExpectedHumid", values);

        // 2. Update sensor to point to the new ExpectedHumid
        await _sqlUpdate.UpdateAsync("Measurements.Sensor",
            new Dictionary<string, object> { { "ExpectedHumidId", expectedHumidId } },
            new Dictionary<string, object> { { "Id", request.SensorId } });

        return Ok(new { message = "ExpectedHumid created and sensor updated", expectedHumidId });
    }
}
