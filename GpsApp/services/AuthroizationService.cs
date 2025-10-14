using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

public interface IAuthorizationService
{
    /// <summary>
    /// Extracts the user ID from the ClaimsPrincipal's claims.
    /// </summary>
    /// <param name="user">The ClaimsPrincipal containing the user claims.</param>
    /// <returns>The user ID as an integer if present and valid; otherwise, null.</returns>
    Task<int?> GetUserIdFromClaims(ClaimsPrincipal user);

    /// <summary>
    /// Verifies if the specified user owns the device with the given device ID.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="gatewayId">The device ID to verify ownership for.</param>
    /// <returns>True if the user owns the device; otherwise, false.</returns>
    Task<bool> VerifyDeviceOwnership(int userId, int gatewayId);


    Task<int?> GetCompanyIdFromClaims(ClaimsPrincipal user);

    Task<bool> HasAccessToDevice(int userId, string role, int companyId, int gatewayId);

    Task<bool> UserCanAccessDevice(ClaimsPrincipal user, int gatewayId);
}

public class AuthorizationService : IAuthorizationService
{
    private readonly ISqlGet _getService;
    private readonly ISqlGetAdvanced _SqlGetAdvanced;

    public AuthorizationService(ISqlGet getService, ISqlGetAdvanced sqlGetAdvanced)
    {
        _getService = getService ?? throw new ArgumentNullException(nameof(getService));
        _SqlGetAdvanced = sqlGetAdvanced ?? throw new ArgumentNullException(nameof(sqlGetAdvanced));
    }

    /// <inheritdoc/>
    public Task<int?> GetUserIdFromClaims(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("userId");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Task.FromResult<int?>(null);
        return Task.FromResult<int?>(userId);
    }

    public Task<int?> GetCompanyIdFromClaims(ClaimsPrincipal user)
    {
        var companyIdClaim = user.FindFirst("companyId");
        if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out var companyId))
            return Task.FromResult<int?>(null);
        return Task.FromResult<int?>(companyId);
    }

    /// <inheritdoc/>
    public async Task<bool> VerifyDeviceOwnership(int userId, int gatewayId)
    {
        var ownership = await _getService.FetchAsync("Secrets.Gateway", new Dictionary<string, object> { { "Id", gatewayId } });
        if (ownership == null) return false;

        var deviceOwnerId = Convert.ToInt32(ownership["UserId"]);
        return deviceOwnerId == userId;
    }

    public async Task<bool> HasAccessToDevice(int userId, string role, int companyId, int gatewayId)
    {
        var baseTable = "Secrets.Gateway g";
        var selectClause = "g.UserId AS OwnerId, a.CompanyId";
        var joins = new List<string>
        {
            "JOIN Secrets.Account a ON a.Id = g.UserId"
        };
        var filters = new Dictionary<string, object>
        {
            { "g.Id", gatewayId }
        };

        var results = await _SqlGetAdvanced.FetchWithJoinsAsync<Dictionary<string, object>>(
            baseTable,
            selectClause,
            joins,
            filters
        );


        if (results == null || results.Count == 0)
            return false;


        var ownerId = Convert.ToInt32(results[0]["OwnerId"]);
        var deviceCompanyId = Convert.ToInt32(results[0]["CompanyId"]);

        // Scenario 1: User owns the devic
        if (ownerId == userId)
            return true;

        // Scenario 2: Admin in same company
        if (role == "Admin" && companyId == deviceCompanyId)
            return true;
        // No access
        return false;
    }

    public async Task<bool> UserCanAccessDevice(ClaimsPrincipal user, int gatewayId)
    {
        var userId = await GetUserIdFromClaims(user);
        var companyId = await GetCompanyIdFromClaims(user);
        var role = user.FindFirst(ClaimTypes.Role)?.Value;

        if (userId == null || companyId == null || string.IsNullOrEmpty(role))
            return false;

        return await HasAccessToDevice(userId.Value, role, companyId.Value, gatewayId);
    }



}
