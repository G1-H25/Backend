using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace GpsApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly string _connectionString;

        public HealthController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("SELECT 1", connection))
                    {
                        command.ExecuteScalar();
                    }
                }
                return Ok("Healthy");
            }
            catch (SqlException)
            {
                // DB not ready or unavailable
                return StatusCode(503, "Database not ready");
            }
        }
    }
}
