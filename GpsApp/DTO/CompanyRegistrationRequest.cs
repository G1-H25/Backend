namespace GpsApp.DTO
{
    public class CompanyRegistrationRequest
    {
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public int ContactId { get; set; }
        public AddressDto Address { get; set; }
    }
}
