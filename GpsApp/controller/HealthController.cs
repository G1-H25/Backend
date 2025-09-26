using Microsoft.AspNetCore.Mvc;

// this is a real controller used to see that the app is alive
namespace GpsApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("Healthy");
    }
}