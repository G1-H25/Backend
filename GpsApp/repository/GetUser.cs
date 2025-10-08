using Microsoft.Data.SqlClient;
using GpsApp.Model;

public class GetUser
{
    private readonly SqlGet _getService;

    public GetUser(SqlGet getService)
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
            DateCreated = Convert.ToDateTime(result["DateCreated"])
        };
    }
}

