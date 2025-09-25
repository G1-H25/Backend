using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GpsApp.Model;
using GpsApp.Configuration;
using GpsApp.DTO;



[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly GetUser _getUserService;

    public LoginController(IConfiguration config, GetUser getUserService)
    {
        _config = config;
        _getUserService = getUserService;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Username and password are required.");

        var user = await _getUserService.GetUserByUsernameAsync(request.Username);

        if (user == null || user.Password != request.Password)
            return Unauthorized("Invalid credentials");

        // You can later add role lookup here, for now just "User"
        var token = GenerateJwtToken(user.Username, user.Role);
        return Ok(new { token });
    }

    private string GenerateJwtToken(string username, string role)
    {
        var issuer = _config.GetResolvedIssuer();
        var audience = _config.GetResolvedAudience();
        var secret = _config.GetResolvedSecretKey();

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
