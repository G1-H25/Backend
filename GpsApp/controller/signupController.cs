using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;


/// <summary>
/// User signup and account creation controller using dependency inversion principle.
/// </summary>
/// <remarks>
/// Handles user account registration with role-based access control.
/// Demonstrates SOLID: SRP (user management), OCP (interface-based), LSP (substitutable),
/// ISP (focused interfaces), DIP (abstraction over concretions).
/// Supports Admin and User roles with security validation.
/// Constructor injection enables testing and loose coupling.
/// </remarks>
[ApiController]
[Route("[controller]")]
public class SignupController : ControllerBase
{
    private readonly ISqlInsert _insertService;

    // roles that are valid
    private static readonly HashSet<string> ValidRoles = new() { "Admin", "User" };

    /// <summary>
    /// Initializes signup controller with injected dependencies.
    /// </summary>
    /// <param name="insertService">Database insertion service (ISqlInsert).</param>
    /// <remarks>Constructor injection - dependencies provided by DI container for testability. Creates accounts with role validation.</remarks>
    public SignupController(ISqlInsert insertService)
    {
        _insertService = insertService;
    }

    [HttpPost()]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Username and password are required.");

        if (string.IsNullOrWhiteSpace(request.Role) || !ValidRoles.Contains(request.Role))
            return BadRequest("Invalid or missing role.");

        if (request.CompanyId <= 0)
            return BadRequest("CompanyId is required and must be greater than 0.");

        var data = new Dictionary<string, object>
        {
            ["AccountUsername"] = request.Username,
            ["AccountPassword"] = request.Password,
            ["AccountRole"] = request.Role,
            ["CompanyId"] = request.CompanyId,
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

