namespace GpsApp.DTO
{
    public class SensorDto
    {
        public int GatewayId { get; set; }
        public Guid UUID { get; set; }
        public DateTime PolledAt { get; set; }
        public float? TemperatureCel { get; set; }
        public float? HumdityPct { get; set; }
    }
}