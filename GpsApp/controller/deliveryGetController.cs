using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.ComponentModel.Design;

[ApiController]
[Route("[controller]")]
public class DeliveryGetController : ControllerBase
{
    private readonly SqlGetAdvanced _sqlAdvanced;

    public DeliveryGetController(SqlGetAdvanced sqlAdvanced)
    {
        _sqlAdvanced = sqlAdvanced;
    }

    [HttpGet]
    public async Task<IActionResult> GetDelivery([FromQuery] int? id)
    {
        var filters = new Dictionary<string, object>();
        if (id.HasValue)
            filters.Add("d.Id", id.Value);

        var result = await _sqlAdvanced.FetchWithJoinsAsync(
            baseTable: "Orders.Delivery d",
            selectClause: @"
                d.Id, d.RouteId, d.ExpectedTempId, d.ExpectedHumidId, d.CarrierId,
                d.SenderId, d.RecipientId, d.StateId, d.OrderPlaced",
            joins: new List<string>
            {
                "FULL JOIN Logistics.Route r ON r.Id = d.RouteId",
                "FULL JOIN Measurements.ExpectedTemp t ON t.Id = d.ExpectedTempId",
                "FULL JOIN Measurements.ExpectedHumid h ON h.Id = d.ExpectedHumidId",
                "FULL JOIN Logistics.Carrier c ON c.Id = d.CarrierId",
                "FULL JOIN Customers.Sender s ON s.Id = d.SenderId",
                "FULL JOIN Customers.Recipient rc ON rc.Id = d.RecipientId",
                "FULL JOIN DeliveryState ds ON ds.Id = d.StateId"
            },
            filters: filters,
            map: r => new DeliveryDto(
                Id: Convert.ToInt32(r["Id"]),
                RouteId: Convert.ToInt32(r["RouteId"]),
                ExpectedTempId: Convert.ToInt32(r["ExpectedTempId"]),
                ExpectedHumidId: Convert.ToInt32(r["ExpectedHumidId"]),
                CarrierId: Convert.ToInt32(r["CarrierId"]),
                SenderId: Convert.ToInt32(r["SenderId"]),
                RecipientId: Convert.ToInt32(r["RecipientId"]),
                StateId: Convert.ToInt32(r["StateId"]),
                OrderPlaced: Convert.ToDateTime(r["OrderPlaced"])
            )
        );

        return result.Any() ? Ok(result) : NotFound("No delivery records found.");
    }
}

