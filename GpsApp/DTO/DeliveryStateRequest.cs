namespace GpsApp.DTO
{
    public class DeliveryStateCreateRequest
    {
        public int DeliveryId { get; set; }
        public string CurrentState { get; set; } = null!;
        public DateTime UpdatedAt { get; set; }
    }
}
