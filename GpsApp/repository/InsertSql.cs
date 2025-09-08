using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;  // Needed for Dictionary
using System.Linq;                 // Needed for data.Keys.Select()

public class SqlInsert
{
    private readonly string _connectionString;

    public SqlInsert(string connectionString)
    {
        _connectionString = connectionString 
            ?? throw new ArgumentNullException(nameof(connectionString));
    }


    public async Task InsertAsync(string tableName, Dictionary<string, object> data)
    {
        var columns = string.Join(", ", data.Keys);
        var paramNames = string.Join(", ", data.Keys.Select(k => "@" + k));

        var sql = $"INSERT INTO {tableName} ({columns}) VALUES ({paramNames})";

        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(sql, conn);

        foreach (var kvp in data)
        {
            cmd.Parameters.AddWithValue("@" + kvp.Key, kvp.Value ?? DBNull.Value);
        }

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
}

