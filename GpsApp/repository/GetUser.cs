using Microsoft.Data.SqlClient;
using GpsApp.Model;

public class GetUser
{
    private readonly ISqlGet _getService;

    public GetUser(ISqlGet getService)
    {
        _getService = getService;
    }

    public async Task<UserData?> GetUserByUsernameAsync(string username)
    {
        var result = await _getService.FetchAsync("Secrets.Account", new Dictionary<string, object>
        {
            { "AccountUsername", username }
        });

        if (result == null) return null;

        return new UserData
        {
            Id = Convert.ToInt32(result["Id"]),
            Username = result["AccountUsername"].ToString()!,
            Password = result["AccountPassword"].ToString()!,
            Role = result["AccountRole"].ToString()!,
            DateCreated = Convert.ToDateTime(result["DateCreated"]),
            CompanyId = result["CompanyId"] != DBNull.Value ? Convert.ToInt32(result["CompanyId"]) : 0 // returns a result if DB is not null
        };
    }
}

