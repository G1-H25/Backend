CREATE OR ALTER PROCEDURE Measurements.UpdateTempTimeOutside
AS
BEGIN
    SET NOCOUNT ON;

    PRINT 'Procedure started...';

    UPDATE sens
    SET sens.TempTimerStart = COALESCE(sens.TempTimerStart, sens.PolledAt)
    FROM Measurements.Sensor sens
    JOIN Measurements.ExpectedTemp temp ON temp.Id = sens.Id
    WHERE sens.CurrentTemp NOT BETWEEN temp.Min AND temp.Max;

    PRINT CONCAT('Step 1 done. Rows affected: ', @@ROWCOUNT);

    UPDATE sens
    SET 
        sens.TempTimeOutside = DATEDIFF(MINUTE, sens.TempTimerStart, sens.PolledAt),
        sens.TempTimerStart = NULL
    FROM Measurements.Sensor sens
    JOIN Measurements.ExpectedTemp temp ON temp.Id = sens.Id
    WHERE sens.TempTimerStart IS NOT NULL
      AND sens.CurrentTemp BETWEEN temp.Min AND temp.Max;

    PRINT CONCAT('Step 2 done. Rows affected: ', @@ROWCOUNT);

    PRINT 'Procedure finished.';
END
GO