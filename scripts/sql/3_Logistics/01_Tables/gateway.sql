IF OBJECT_ID('Gateway', 'U') IS NULL
    BEGIN
        CREATE TABLE Gateway (
            Id INT IDENTITY (1, 1),
            Field VARCHAR(50),
            PRIMARY KEY (Id),
        )
    END
GO
