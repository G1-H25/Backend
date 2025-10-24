DECLARE @UserId INT, @CurrentLocationId INT;
SET @UserId = (SELECT Id FROM Secrets.Account WHERE AccountUsername = 'Admin');
SET @CurrentLocationId = (SELECT Id FROM Secrets.LocationHistory WHERE PolledAt = '2025-10-04T14:30:00');

INSERT INTO Secrets.Gateway (UUID, GatewayURL, UserId, CurrentLocationId)
VALUES (
    'A3E77B65-2D8B-4C8A-9C9B-8C9A6E4D6F9E', 
    'http://gateway.simulated.iot',
    @UserId,
    @CurrentLocationId
);
