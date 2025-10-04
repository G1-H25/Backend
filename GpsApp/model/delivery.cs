public record DeliveryData (
    int deliveryId, 
    int routeID, 
    int ExpectedTempId, 
    int ExpectedHumidId, 
    int carrierID, 
    int senderId, 
    int recipientId,
    int stateId, 
    DateTime? OrderPlaced 
); 
