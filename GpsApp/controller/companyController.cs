using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;



[ApiController]
[Route("[controller]")]
public class CompanyController : ControllerBase
{
    private readonly SqlInsert _insertService;

    // get the connectionstring to azure database
    public CompanyController(SqlInsert insertService)
    {
        _insertService = insertService;
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
}
