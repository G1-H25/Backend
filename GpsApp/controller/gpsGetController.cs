using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

[ApiController]
[Route("[controller]")]
public class GpsGetController : ControllerBase
{
    private readonly SqlGet _getService;

    public GpsGetController(SqlGet getService)
    {
        _getService = getService;
    }



    [HttpGet]
    public async Task<IActionResult> GetGpsData([FromQuery] GpsData data)
    {
        var filters = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(data.DeviceId))
            filters.Add("DeviceId", data.DeviceId);

        if (data.Timestamp.HasValue)
            filters.Add("Timestamp", data.Timestamp.Value);

        if (!filters.Any())
            return BadRequest("At least one filter parameter must be provided.");

        var result = await _getService.FetchAsync("GpsData", filters);

        if (result == null)
            return NotFound("No matching GPS data found.");

        return Ok(result);
    }
}