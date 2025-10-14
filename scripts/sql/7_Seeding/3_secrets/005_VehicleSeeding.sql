DECLARE @GatewayId INT, @RegistrationId INT;
SET @GatewayId = (SELECT Id FROM Secrets.Account WHERE AccountUsername = 'Admin');
SET @RegistrationId = (SELECT Id FROM Secrets.Registration WHERE Plate = 'AAA111');

BEGIN
    INSERT INTO Secrets.Vehicle (GatewayId, RegistrationId)
    VALUES (@GatewayId, @RegistrationId)
END
GO