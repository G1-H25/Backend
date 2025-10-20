namespace GpsApp.DTO
{
    public class DeliveryCreateRequest
    {
        public int RouteId { get; set; }
        public int SensorId { get; set; }
        public ExpectedTempRequest ExpectedTemp { get; set; }
        public ExpectedHumidRequest ExpectedHumid { get; set; }
        public int RecipientId { get; set; }
        public int SenderId { get; set; }
        public int CarrierId { get; set; }
        public DateTime OrderPlaced { get; set; }
    }

    public class ExpectedTempRequest
    {
        public string Note { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
    }

    public class ExpectedHumidRequest
    {
        public string Note { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
    }

}
