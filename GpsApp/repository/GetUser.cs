using Microsoft.Data.SqlClient;
using GpsApp.Model;

public class GetUser
{
    private readonly SqlGet _getService;

    public GetUser(SqlGet getService)
    {
        _getService = getService;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        var result = await _getService.FetchAsync("Secrets.Account", new Dictionary<string, object>
        {
            { "Username", username }
        });

        if (result == null) return null;

        return new User
        {
            Id = Convert.ToInt32(result["Id"]),
            Username = result["Username"].ToString()!,
            Password = result["Password"].ToString()!,
            Role = result["Role"].ToString()!,
            DateCreated = Convert.ToDateTime(result["DateCreated"])
        };
    }
}

