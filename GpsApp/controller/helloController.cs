using Microsoft.AspNetCore.Mvc;

namespace GpsApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HelloController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello from the ASP.NET Core API!");
        }
    }
}