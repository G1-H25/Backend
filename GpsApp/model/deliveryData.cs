public record DeliveryData (
    int? DeliveryId, 
    int? RouteId, 
    int? ExpectedTempId, 
    int? ExpectedHumidId, 
    int? CarrierId, 
    int? SenderId, 
    int? RecipientId,
    int? StateId, 
    DateTime? OrderPlaced
);
