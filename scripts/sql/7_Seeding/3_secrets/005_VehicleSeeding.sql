DECLARE @GatewayId INT, @RegistrationId INT;

-- Get the GatewayId from the seeded gateway
SET @GatewayId = (SELECT Id FROM Secrets.Gateway WHERE UUID = 'A3E77B65-2D8B-4C8A-9C9B-8C9A6E4D6F9E');

-- Get the RegistrationId
SET @RegistrationId = (SELECT Id FROM Secrets.Registration WHERE Plate = 'AAA111');

-- Insert into Vehicle
INSERT INTO Secrets.Vehicle (GatewayId, RegistrationId)
VALUES (@GatewayId, @RegistrationId);
GO
