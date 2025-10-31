namespace GpsApp.DTO
{
    public class CurrentLocationHistoryCreate
    {
        public int GatewayId { get; set; }
        public DateTime PolledAt { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }

}
