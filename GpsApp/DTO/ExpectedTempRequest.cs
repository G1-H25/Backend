public class ExpectedTempCreateRequest
{
    public int SensorId { get; set; }       // To know which sensor to update
    public string Note { get; set; }
    public decimal Min { get; set; }
    public decimal Max { get; set; }
}
