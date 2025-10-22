using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;

[ApiController]
[Route("[controller]")]
public class SenderController : ControllerBase
{
    private readonly SqlInsert _insertService;

    public SenderController(SqlInsert insertService)
    {
        _insertService = insertService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateSender([FromBody] SenderCreateRequest request)
    {
        if (request.CompanyId <= 0)
            return BadRequest("Invalid CompanyId.");

        var values = new Dictionary<string, object>
        {
            { "CompanyId", request.CompanyId }
        };

        int id = await _insertService.InsertAndReturnIdAsync("Logistics.Sender", values);

        return Ok(new { message = "Sender created", id });
    }
}
