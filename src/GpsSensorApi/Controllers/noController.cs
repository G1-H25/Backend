using Microsoft.AspNetCore.Mvc;

namespace GpsSensorApi.Controllers
{
    [ApiController]
    [Route("[controller]")] // removes controller part of the route words
    public class NoController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("say no!");
        }
    }
}
