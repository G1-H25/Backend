using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;  // Needed for Dictionary
using System.Linq;                 // Needed for data.Keys.Select()

public interface ISqlInsert
{
    /// <summary>
    /// Gets the database connection string for direct SQL access scenarios.
    /// </summary>
    /// <remarks>Provides connection string for advanced cases requiring raw SQL operations. Use cautiously as it breaks abstraction layer.</remarks>
    string ConnectionString { get; }

    /// <summary>
    /// Inserts data into the specified table.
    /// </summary>
    /// <param name="tableName">Target table name.</param>
    /// <param name="data">Column-value pairs to insert.</param>
    /// <returns>Async task.</returns>
    Task InsertAsync(string tableName, Dictionary<string, object> data);

    /// <summary>
    /// Inserts data and returns the generated identity value.
    /// </summary>
    /// <param name="tableName">Target table name.</param>
    /// <param name="data">Column-value pairs to insert.</param>
    /// <returns>Generated identity value.</returns>
    Task<int> InsertAndReturnIdAsync(string tableName, Dictionary<string, object> data);
}

public class SqlInsert : ISqlInsert
{
    private readonly string _connectionString;

    public SqlInsert(string connectionString)
    {
        _connectionString = connectionString
            ?? throw new ArgumentNullException(nameof(connectionString));
    }

    // exposes the string for read only
    public string ConnectionString => _connectionString;

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

    public async Task<int> InsertAndReturnIdAsync(string tableName, Dictionary<string, object> data)
    {
        var columns = string.Join(", ", data.Keys);
        var paramNames = string.Join(", ", data.Keys.Select(k => "@" + k));

        var sql = $@"
            INSERT INTO {tableName} ({columns}) VALUES ({paramNames});
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(sql, conn);

        foreach (var kvp in data)
        {
            cmd.Parameters.AddWithValue("@" + kvp.Key, kvp.Value ?? DBNull.Value);
        }

        await conn.OpenAsync();
        var result = await cmd.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }
}

