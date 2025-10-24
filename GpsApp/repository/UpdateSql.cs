using Microsoft.Data.SqlClient;

public interface ISqlUpdate
{
    Task UpdateAsync(string tableName, Dictionary<string, object> setValues, Dictionary<string, object> filters);
}

public class SqlUpdate : ISqlUpdate
{
    private readonly string _connectionString;

    public SqlUpdate(string connectionString)
    {
        _connectionString = connectionString
            ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task UpdateAsync(string tableName, Dictionary<string, object> setValues, Dictionary<string, object> filters)
    {
        var updates = string.Join(", ", setValues.Select(kvp => $"{kvp.Key} = @{kvp.Key}"));
        var where = string.Join(" AND ", filters.Select(kvp => $"{kvp.Key} = @filter_{kvp.Key}"));

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"UPDATE {tableName} SET {updates} WHERE {where}";

        foreach (var kvp in setValues)
            cmd.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);

        foreach (var kvp in filters)
            cmd.Parameters.AddWithValue($"@filter_{kvp.Key}", kvp.Value ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
    }
}
