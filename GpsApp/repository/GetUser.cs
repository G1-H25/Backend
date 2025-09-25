using Microsoft.Data.SqlClient;
using GpsApp.Model;

public class GetUser
{

    private readonly string _connectionString;

    public GetUser(string connectionString)
    {
        _connectionString = connectionString
            ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        var query = "SELECT Username, Password, Role, DateCreated FROM dbo.Users WHERE Username = @Username";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@Username", username);
        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                Username = reader.GetString(0),
                Password = reader.GetString(1),
                Role = reader.GetString(2),
                DateCreated = reader.GetDateTime(3)
            };
        }

        return null;
    }
}