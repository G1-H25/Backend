using Microsoft.Data.SqlClient;
using System.Data;

public class SqlGetAdvanced
{
    private readonly string _connectionString;

    public SqlGetAdvanced(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<T>> FetchWithJoinsAsync<T>(
        string baseTable,
        string selectClause,
        List<string> joins,
        Dictionary<string, object>? filters = null,
        Func<IDataRecord, T>? map = null)
    {
        var results = new List<T>();

        // 1. SELECT clause
        string sql = $"SELECT {selectClause} FROM {baseTable}";

        // 2. JOINs
        if (joins.Any())
            sql += " " + string.Join(" ", joins);

        // 3. WHERE clause
        if (filters != null && filters.Any())
        {
            var whereClause = string.Join(" AND ", filters.Keys.Select(k => $"{k} = @{k}"));
            sql += $" WHERE {whereClause}";
        }

        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(sql, conn);

        // 4. Parameters
        if (filters != null)
        {
            foreach (var (key, value) in filters)
            {
                cmd.Parameters.AddWithValue("@" + key, value ?? DBNull.Value);
            }
        }

        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();

        // 5. Row mapping
        while (await reader.ReadAsync())
        {
            if (map != null)
            {
                results.Add(map(reader));
            }
            else
            {
                // Fallback: dynamic object
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null! : reader.GetValue(i);

                results.Add((T)(object)row);
            }
        }

        return results;
    }
}
