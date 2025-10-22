namespace GpsApp.DTO
{
    public class ExpectedHumidCreateRequest
    {
        public int SensorId { get; set; }
        public string Note { get; set; } = string.Empty;
        public decimal Min { get; set; }
        public decimal Max { get; set; }
    }
}
