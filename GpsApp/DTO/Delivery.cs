namespace GpsApp.DTO
{
    public record DeliveryDto(
        int DeliveryId,
        string RouteCode,
        float TempMin,
        float TempMax,
        float HumidMin,
        float HumidMax,
        string CarrierName,
        string SenderName,
        string RecipientName,
        DateTime OrderPlaced
    );
}