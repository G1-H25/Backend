CREATE OR ALTER PROCEDURE Measurements.UpdateHumidTimeOutside
AS
BEGIN
    SET NOCOUNT ON;

    PRINT 'Procedure started...';

    UPDATE sens
    SET sens.HumidTimerStart = COALESCE(sens.HumidTimerStart, sens.PolledAt)
    FROM Measurements.Sensor sens
    JOIN Measurements.ExpectedHumid humid ON humid.Id = sens.Id
    WHERE sens.CurrentHumid NOT BETWEEN humid.Min AND humid.Max;

    PRINT CONCAT('Step 1 done. Rows affected: ', @@ROWCOUNT);

    UPDATE sens
    SET 
        sens.HumidTimeOutside = DATEDIFF(MINUTE, sens.HumidTimerStart, sens.PolledAt),
        sens.HumidTimerStart = NULL
    FROM Measurements.Sensor sens
    JOIN Measurements.ExpectedHumid humid ON humid.Id = sens.Id
    WHERE sens.HumidTimerStart IS NOT NULL
      AND sens.CurrentHumid BETWEEN humid.Min AND humid.Max;

    PRINT CONCAT('Step 2 done. Rows affected: ', @@ROWCOUNT);

    PRINT 'Procedure finished.';
END
GO