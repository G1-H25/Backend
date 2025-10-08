/*
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

    // curently requires postaddress to be sent in with the request, which may not be ideal since it is a foreign key

    [HttpPost("register")]
    public async Task<IActionResult> RegisterCompany([FromBody] CompanyRegistrationRequest data)
    {
        if (string.IsNullOrWhiteSpace(data.CompanyName))
            return BadRequest("CompanyName is required.");

        if (string.IsNullOrWhiteSpace(data.Email))
            return BadRequest("Email is required.");

        if (string.IsNullOrWhiteSpace(data.Street))
            return BadRequest("Street is required.");

        if (data.StreetNumber <= 0)
            return BadRequest("StreetNumber must be a positive integer.");

        // Optional: you could validate ContactId and PostAddressId against the database here

        var values = new Dictionary<string, object>
        {
            { "CompanyName", data.CompanyName },
            { "Email", data.Email },
            { "Street", data.Street },
            { "StreetNumber", data.StreetNumber },
            { "ContactId", data.ContactId },
            { "PostAddressId", data.PostAddressId }
        };

        await _insertService.InsertAsync("Customers.Company", values);

        return Ok("Company inserted.");
    }
}
*/