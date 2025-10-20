namespace GpsApp.DTO
{

    public record DeliveryDto(
        int DeliveryId,
        string RouteCode,
        float CurrentTemp,
        float ExpectedTempMin,
        float ExpectedTempMax,
        float TempMinMeasured,
        float TempMaxMeasured,
        float TempOutOfRange,
        float CurrentHumid,
        float ExpectedHumidMin,
        float ExpectedHumidMax,
        float HumidMinMeasured,
        float HumidMaxMeasured,
        float HumidOutOfRange,
        string Carrier,
        string Sender,
        string Recipient,
        DateTime OrderPlaced,
        StatusDto Status
    );
}

public record StatusDto(string Text, string Timestamp);