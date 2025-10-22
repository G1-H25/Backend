using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;

[ApiController]
[Route("[controller]")]
public class ExpectedHumidController : ControllerBase
{
    private readonly SqlInsert _insertService;
    private readonly SqlUpdate _sqlUpdate;

    public ExpectedHumidController(SqlInsert insertService, SqlUpdate sqlUpdate)
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
