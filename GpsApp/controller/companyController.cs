using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;



/// <summary>
/// Company registration controller using dependency inversion principle.
/// </summary>
/// <remarks>
/// Handles company setup and management operations.
/// Demonstrates SOLID: SRP (company management), OCP (interface-based), LSP (substitutable),
/// ISP (focused interfaces), DIP (abstraction over concretions).
/// Constructor injection enables testing and loose coupling.
/// </remarks>
[ApiController]
[Route("[controller]")]
public class CompanyController : ControllerBase
{
    private readonly ISqlInsert _insertService;
    private readonly ISqlGet _getService;

    /// <summary>
    /// Initializes company controller with injected dependencies.
    /// </summary>
    /// <param name="insertService">Database insertion service (ISqlInsert).</param>
    /// <param name="getService">Database query service (ISqlGet).</param>
    /// <remarks>Constructor injection - dependencies provided by DI container for testability.</remarks>
    public CompanyController(ISqlInsert insertService, ISqlGet getService)
    {
        _insertService = insertService;
        _getService = getService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterCompany([FromBody] CompanyRegistrationRequest data)
    {
        if (string.IsNullOrWhiteSpace(data.CompanyName) || string.IsNullOrWhiteSpace(data.Email))
            return BadRequest("CompanyName and Email are required.");

        // 1. Insert address first
        var addressValues = new Dictionary<string, object>
        {
            { "Street", data.Address.Street },
            { "StreetNumber", data.Address.StreetNumber },
            { "ZipCode", data.Address.PostalCode },
            { "Locality", data.Address.City },
            { "Country", data.Address.Country }
        };


        int postAddressId = await _insertService.InsertAndReturnIdAsync("Customers.PostAddress", addressValues);

        // 2. Insert company
        var companyValues = new Dictionary<string, object>
        {
            { "CompanyName", data.CompanyName },
            { "Email", data.Email },
            { "PostAddressId", postAddressId }
        };

        int companyId = await _insertService.InsertAndReturnIdAsync("Customers.Company", companyValues);

        // Return JSON response with companyId
        return Ok(new { message = "Company inserted.", companyId });
    }

    [HttpGet("id")]
    public async Task<IActionResult> GetCompanyIdByFilters(
        [FromQuery] string? name,
        [FromQuery] string? email,
        [FromQuery] int? postAddressId)
    {
        var filters = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(name))
            filters.Add("CompanyName", name);

        if (!string.IsNullOrWhiteSpace(email))
            filters.Add("Email", email);

        if (postAddressId.HasValue && postAddressId.Value > 0)
            filters.Add("PostAddressId", postAddressId.Value);

        if (filters.Count == 0)
            return BadRequest("At least one filter parameter is required.");

        var result = await _getService.FetchAsync(
            tableName: "Customers.Company",
            filters: filters,
            columns: new[] { "Id" }
        );

        if (result == null)
            return NotFound("Company not found.");

        return Ok(new { Id = Convert.ToInt32(result["Id"]) });
    }


}
