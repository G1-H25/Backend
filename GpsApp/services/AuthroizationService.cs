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
    /// <param name="deviceId">The device ID to verify ownership for.</param>
    /// <returns>True if the user owns the device; otherwise, false.</returns>
    Task<bool> VerifyDeviceOwnership(int userId, string deviceId);
}

public class AuthorizationService : IAuthorizationService
{
    private readonly SqlGet _getService;

    public AuthorizationService(SqlGet getService)
    {
        _getService = getService;
    }

    /// <inheritdoc/>
    public Task<int?> GetUserIdFromClaims(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("userId");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Task.FromResult<int?>(null);
        return Task.FromResult<int?>(userId);
    }

    /// <inheritdoc/>
    public async Task<bool> VerifyDeviceOwnership(int userId, string deviceId)
    {
        var ownership = await _getService.FetchAsync("Secrets.Gateway", new Dictionary<string, object> { { "Id", deviceId } });
        if (ownership == null) return false;

        var deviceOwnerId = Convert.ToInt32(ownership["UserId"]);
        return deviceOwnerId == userId;
    }
}
