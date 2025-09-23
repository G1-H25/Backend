IF OBJECT_ID('Delivery', 'U') IS NULL
BEGIN
    CREATE TABLE Delivery(
        Id INT IDENTITY(1,1),
        Date NUMERIC NOT NULL DEFAULT CURRENT_TIMESTAMP, 
        PRIMARY KEY (Id), 
        FOREIGN KEY (CarrierId) REFERENCES Carrier(Id), 
        FOREIGN KEY (SenderId) REFERENCES Sender (Id),
        FOREIGN KEY(RecipientId) REFERENCES Recipient(Id), 
        FOREIGN KEY(ExpectedMoisId) REFERENCES ExpectedMois(Id), 
        FOREIGN KEY(ExpectedTempId) REFERENCES ExpectedTemp(Id) 
    )
END
GO
