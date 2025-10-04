using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.ComponentModel.Design;

[ApiController]
[Route("[controller]")]
public class DeliveryGetController : ControllerBase
{
    private readonly string _connectionString;

    public DeliveryGetController(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    [HttpGet]
    public async Task<IActionResult> GetDeliveryData([FromQuery] DeliveryData data)
    {
        string sql = (
                @"SELECT Id, RouteId, ExpectedTempId, ExpectedHumidId, CarrierId, SenderId, RecipientId, StateId, OrderPlaced
                FROM Orders.Delivery
                FULL JOIN Logistics.Route ON  Logistics.Route.Id = Orders.Delivery.RouteId
                FULL JOIN Measurements ON Measurements.ExpectedTemp.Id = Orders.Delivery.ExpectedTempId
                FULL JOIN Measurements.ExpectedHumid ON Measurements.ExpectedHumid(Id) = Orders.Delivery.ExpectedHumidId
                FULL JOIN Logistics.Carrier ON Logistics.Carrier(Id) = Orders.Delivery.CarrierId
                FULL JOIN Customers.Sender ON Customers.Sender(Id) = Orders.Delivery.SenderId
                FULL JOIN Customers.Recipient ON Customers.Recipient(Id) = Orders.Delivery.RecipientId
                FULL JOIN DeliveryState ON DeliveryState(Id) = Orders.Delivery.StateId
        ");
        await using var connection = new SqlConnection(_getService);
        await using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@", data);

        await connection.OpenAsync();

        await using var reader = await command.ExecuteReaderAsync();


    }
    
}
