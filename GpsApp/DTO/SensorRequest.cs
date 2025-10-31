namespace GpsApp.DTO
{
    public class SensorDto
    {
        public Guid GatewayUUID { get; set; }
        public Guid UUID { get; set; }
        public DateTime PolledAt { get; set; }
        public float? TemperatureCel { get; set; }
        public float? HumdityPct { get; set; }
    }

    // Helper class to represent sensor reading data for database operations
    public class SensorReading
    {
        public DateTime? TempTimerStart { get; set; }
        public int TempTimeOutside { get; set; }
        public DateTime? HumidTimerStart { get; set; }
        public int HumidTimeOutside { get; set; }
        public DateTime PolledAt { get; set; }
        public float TempMinMeasured { get; set; }
        public float TempMaxMeasured { get; set; }
        public float HumidMinMeasured { get; set; }
        public float HumidMaxMeasured { get; set; }
    }
}