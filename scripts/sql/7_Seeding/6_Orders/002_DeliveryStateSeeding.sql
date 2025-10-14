DECLARE @DeliveryId INT, @Now DATETIME2 = SYSDATETIME();
SET @DeliveryId = (
    SELECT TOP 1 deliv.Id 
    FROM Orders.Delivery deliv
    JOIN Logistics.Carrier carr ON deliv.CarrierId = carr.Id
    JOIN Secrets.Registration reg ON carr.VehicleId = reg.Id
    WHERE reg.Plate = 'AAA111')

IF
    NOT EXISTS (
        SELECT 1
        FROM Orders.DeliveryState
        WHERE CurrentState = 'Pending'
    )
    BEGIN
        INSERT INTO Orders.DeliveryState
            (DeliveryId, CurrentState, UpdatedAt)
        VALUES
            (@DeliveryId, 'Pending', @Now)
END
GO