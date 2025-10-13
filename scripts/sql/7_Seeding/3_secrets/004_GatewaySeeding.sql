DECLARE @UserId INT, @CurrentLocationId INT;
SET @UserId = (SELECT Id FROM Secrets.Account WHERE AccountUsername = 'Admin');
SET @CurrentLocationId = (SELECT Id FROM Secrets.LocationHistory WHERE PolledAt = '2025-10-04T14:30:00');

BEGIN
INSERT INTO Secrets.Gateway (UserId, CurrentLocationId)
VALUES (@UserId, @CurrentLocationId)
END
GO