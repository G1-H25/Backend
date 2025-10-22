using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;

[ApiController]
[Route("[controller]")]
public class RecipientController : ControllerBase
{
    private readonly SqlInsert _insertService;

    public RecipientController(SqlInsert insertService)
    {
        _insertService = insertService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateRecipient([FromBody] RecipientCreateRequest request)
    {
        if (request.CompanyId <= 0)
            return BadRequest("Invalid CompanyId.");

        var values = new Dictionary<string, object>
        {
            { "CompanyId", request.CompanyId }
        };

        int id = await _insertService.InsertAndReturnIdAsync("Logistics.Recipient", values);

        return Ok(new { message = "Recipient created", id });
    }
}
