using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

[ApiController]
[Route("[controller]")]
public class GpsController : ControllerBase
{
    private readonly SqlInsert _insertService;

    // get the connectionstring to azure database
    public GpsController(SqlInsert insertService)
    {
        _insertService = insertService;
    }


    [HttpPost]
    public async Task<IActionResult> PostGpsData([FromBody] GpsData data)
    {

        if (string.IsNullOrWhiteSpace(data.DeviceId))
            return BadRequest("DeviceId is required.");

        if (data.Latitude < -90 || data.Latitude > 90)
            return BadRequest("Latitude must be between -90 and 90.");

        if (data.Longitude < -180 || data.Longitude > 180)
            return BadRequest("Longitude must be between -180 and 180.");

        if (data.Timestamp == default)
            return BadRequest("Timestamp must be set.");


        var dataDict = new Dictionary<string, object>
        {
            { "DeviceId", data.DeviceId },
            { "Latitude", data.Latitude },
            { "Longitude", data.Longitude },
            { "Timestamp", data.Timestamp }
        };

        await _insertService.InsertAsync("GpsData", dataDict);

        return Ok("Inserted");
    }
}

