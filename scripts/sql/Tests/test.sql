IF OBJECT_ID('dbo.Test', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.test (
            id INT,
            description VARCHAR(50)
        );
    END
GO
