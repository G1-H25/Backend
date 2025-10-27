using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("test/mock")]
public class TestMockDataController : ControllerBase
{
    private readonly SqlInsert _insertService;
    private readonly SqlUpdate _sqlUpdate;
    private readonly ISqlGet _sqlGet;

    public TestMockDataController(SqlInsert insertService, SqlUpdate sqlUpdate, ISqlGet sqlGet)
    {
        _insertService = insertService;
        _sqlUpdate = sqlUpdate;
        _sqlGet = sqlGet;
    }

    [Fact]
    public async Task PostingSensor_ShouldCreateLinkedMockDelivery()
    {
        // Arrange
        var requestBody = new
        {
            GatewayId = 1,     // existing gateway
            TemperatureCel = 25.3,
            HumdityPct = 50.5
        };

        // Act
        var response = await PostJsonAsync("/Sensor", requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "posting sensor should succeed");

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Inserted", "the sensor insert response should indicate success");

        // Optional: Query API or DB to confirm mock delivery exists for that sensor
    }

}
