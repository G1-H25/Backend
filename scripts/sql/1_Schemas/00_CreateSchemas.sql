IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Measurements')
    EXEC('CREATE SCHEMA Measurements');
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Logistics')
    EXEC('CREATE SCHEMA Logistics');
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Orders')
    EXEC('CREATE SCHEMA Orders');
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Customers')
    EXEC('CREATE SCHEMA Customers');
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Secrets')
    EXEC('CREATE SCHEMA Secrets');
GO