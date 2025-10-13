
namespace GpsApp.DTO
{
    public class SignupRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        
        public string Role { get; set; } // "Admin" or "User"
        public int CompanyId { get; set; } // FK to Customers.Company
    }
}