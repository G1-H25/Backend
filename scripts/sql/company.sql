CREATE TABLE Company (
    PK INT IDENTITY(1,1) PRIMARY KEY,
    Id INT IDENTITY(1,1) UNIQUE, -- You usually only need one identity, but included as requested
    Name VARCHAR(100),
    Email VARCHAR(100),
    Street VARCHAR(40),
    StreetNumber INT,
    PostNumberId INT,
    ContactId INT,
    CONSTRAINT FK_Company_PostNumber FOREIGN KEY (PostNumberId) REFERENCES PostNumber(PK),
    CONSTRAINT FK_Company_Contact FOREIGN KEY (ContactId) REFERENCES Contact(PK)
);