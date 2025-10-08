namespace GpsApp.DTO
{
    public class CompanyRegistrationRequest
    {   
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string Street { get; set; }
        public int StreetNumber { get; set; }
        public int ContactId { get; set; }
        public int PostAddressId { get; set; }
    }
}
