using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;


[ApiController]
[Route("[controller]")]
public class TransportRouteController : ControllerBase
{
    private readonly SqlInsert _insertService;

    public TransportRouteController(SqlInsert insertService)
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
