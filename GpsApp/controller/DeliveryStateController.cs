using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;

[ApiController]
[Route("[controller]")]
public class DeliveryStateController : ControllerBase
{
    private readonly SqlInsert _insertService;

    public DeliveryStateController(SqlInsert insertService)
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
