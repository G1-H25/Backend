namespace GpsApp.DTO
{
    public record DeliveryDto(
        int Id,
        string RouteId,
        float ExpectedTempId,
        float ExpectedHumidId,
        float minMaxTemp,
        float minMaxHumid,
        string CarrierId,
        string SenderId,
        string RecipientId,
        string StateId,
        DateTime OrderPlaced
    );
}