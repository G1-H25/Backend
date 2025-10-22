namespace GpsApp.DTO
{
    public class CarrierCreateRequest
    {
        public int CompanyId { get; set; }
        public int GatewayId { get; set; }  // existing gateway

        // Nested registration info
        public string Plate { get; set; } = null!;
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
    }
}
