using Microsoft.AspNetCore.Mvc;

namespace GpsSensorApi.Controllers
{
    [ApiController]
    [Route("[controller]")] // removes controller part of the route word
    public class HelloController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello from the ASP.NET Core API!");
        }
    }
}
