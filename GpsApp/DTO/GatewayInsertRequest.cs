namespace GpsApp.DTO
{
    /*
            CREATE TABLE Secrets.Gateway
    (
        Id INT IDENTITY (1, 1) PRIMARY KEY,
        GatewayURL VARCHAR(50),
        UserId INT NULL CONSTRAINT FK_Gateway_UserId
            FOREIGN KEY (UserId) REFERENCES Secrets.Account(Id),
        CurrentLocationId INT NULL CONSTRAINT FK_Gateway_CurrentLocationId
            FOREIGN KEY (CurrentLocationId) REFERENCES Secrets.LocationHistory(Id)
    )
    */
    public class GatewayInsertRequest
    {
        public Guid UUID { get; set; }
        public string? GatewayURL { get; set; }
        public int? CurrentLocationId { get; set; }
    }
}
