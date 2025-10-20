DECLARE @UserId INT, @CurrentLocationId INT;
SET @UserId = (SELECT Id FROM Secrets.Account WHERE AccountUsername = 'Admin');
SET @CurrentLocationId = (SELECT Id FROM Secrets.LocationHistory WHERE PolledAt = '2025-10-04T14:30:00');


INSERT INTO Secrets.Gateway (Id, GatewayURL, UserId, CurrentLocationId)
VALUES (1, 'http://gateway.simulated.iot', @UserId, @CurrentLocationId);
