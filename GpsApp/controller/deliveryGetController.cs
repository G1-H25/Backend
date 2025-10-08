using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.ComponentModel.Design;

[ApiController]
[Route("[controller]")]
public class DeliveryGetController : ControllerBase
{
    private readonly SqlGetAdvanced _sqlAdvanced;
    private readonly IAuthorizationService _authService;

    public DeliveryGetController(SqlGetAdvanced sqlAdvanced, IAuthorizationService authService)
    {
        _sqlAdvanced = sqlAdvanced;
        _authService = authService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetDelivery([FromQuery] int? id)
    {
        var userId = await _authService.GetUserIdFromClaims(User);
        if (userId == null)
        return Unauthorized("User ID not found in token.");

        var filters = new Dictionary<string, object>();
        if (id.HasValue)
            filters.Add("d.Id", id.Value);

        var result = await _sqlAdvanced.FetchWithJoinsAsync(
            baseTable: "Orders.Delivery deliv",
            selectClause: @"
                deliv.Id AS DeliveryId,
                troute.Code AS RouteCode,
                temp.Min AS TempMin,
                temp.Max AS TempMax,
                humid.Min AS HumidMin,
                humid.Max AS HumidMax,
                carrCom.CompanyName AS CarrierName,
                senCom.CompanyName AS SenderName,
                recCom.CompanyName AS RecipientName,
                deliv.OrderPlaced
            ",
            joins: new List<string>
            {
                "JOIN Logistics.TransportRoute troute ON deliv.RouteId = troute.Id",
                "JOIN Measurements.ExpectedTemp temp ON deliv.ExpectedTempId = temp.Id",
                "JOIN Measurements.ExpectedHumid humid ON deliv.ExpectedHumidId = humid.Id",
                "JOIN Logistics.Recipient rec ON deliv.RecipientId = rec.Id",
                "JOIN Customers.Company recCom ON rec.CompanyId = recCom.Id",
                "JOIN Logistics.Sender sen ON deliv.SenderId = sen.Id",
                "JOIN Customers.Company senCom ON sen.CompanyId = senCom.Id",
                "JOIN Logistics.Carrier carr ON deliv.CarrierId = carr.Id",
                "JOIN Customers.Company carrCom ON carr.CompanyId = carrCom.Id"
            },
            filters: filters,
            map: r => new DeliveryDto(
                DeliveryId: Convert.ToInt32(r["DeliveryId"]),
                RouteCode: Convert.ToString(r["RouteCode"]),
                TempMin: Convert.ToSingle(r["TempMin"]),
                TempMax: Convert.ToSingle(r["TempMax"]),
                HumidMin: Convert.ToSingle(r["HumidMin"]),
                HumidMax: Convert.ToSingle(r["HumidMax"]),
                CarrierName: Convert.ToString(r["CarrierName"]),
                SenderName: Convert.ToString(r["SenderName"]),
                RecipientName: Convert.ToString(r["RecipientName"]),
                OrderPlaced: Convert.ToDateTime(r["OrderPlaced"])
            )
        );

        return result.Any() ? Ok(result) : NotFound("No delivery records found.");
    }
}

