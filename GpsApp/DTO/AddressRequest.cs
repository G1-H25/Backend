// AddressRequest.cs

namespace GpsApp.DTO
{
    public class AddressDto
    {
        public string Street { get; set; }
        public int StreetNumber { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}