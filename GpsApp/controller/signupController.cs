using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;


[ApiController]
[Route("[controller]")]
public class SignupController : ControllerBase
{
    private readonly SqlInsert _insertService;

    public SignupController(SqlInsert insertService)
    {
        _insertService = insertService;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Username and password are required.");

        var data = new Dictionary<string, object>
        {
            ["Username"] = request.Username,
            ["Password"] = request.Password,
            ["Role"] = "User",
            ["DateCreated"] = DateTime.UtcNow
        };

        try
        {
            await _insertService.InsertAsync("Secrets.Account", data);
            return Ok("User created successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"DB error: {ex.Message}");
        }
    }

}

