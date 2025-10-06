namespace GpsApp.DTO
{
    public record DeliveryDto(
        int Id,
        int RouteId,
        int ExpectedTempId,
        int ExpectedHumidId,
        int CarrierId,
        int SenderId,
        int RecipientId,
        int StateId,
        DateTime OrderPlaced
    );
}