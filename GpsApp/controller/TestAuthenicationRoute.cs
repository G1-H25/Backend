using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("user-only")]
    [Authorize(Roles = "User")]
    public IActionResult UserOnlyEndpoint()
    {
        var claims = User.Claims
            .Select(c => $"{c.Type}: {c.Value}")
            .ToList();

        return Ok(new
        {
            Name = User.Identity?.Name ?? "Unknown",
            Claims = claims
        });
    }
}