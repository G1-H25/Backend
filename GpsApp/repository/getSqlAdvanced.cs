using Microsoft.Data.SqlClient;
using System.Data;

public class SqlGetAdvanced
{
    private readonly string _connectionString;

    public SqlGetAdvanced(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Executes a dynamic SELECT query with JOINs, optional WHERE filters, and optional mapping to a custom type.
    /// </summary>
    /// <typeparam name="T">The type to map each row to (e.g., a DTO or Dictionary).</typeparam>
    /// <param name="baseTable">The main table to query from (with optional alias), e.g., "Orders.Delivery d".</param>
    /// <param name="selectClause">The SELECT part of the query (without the "SELECT" keyword), e.g., "d.Id, d.Name".</param>
    /// <param name="joins">A list of JOIN clauses (e.g., "JOIN Customers c ON c.Id = d.CustomerId").</param>
    /// <param name="filters">Optional dictionary of WHERE filters. Key = column name, Value = parameter value. Uses parameterization for safety.</param>
    /// <param name="map">Optional mapping function that converts each IDataRecord (row) into type T. If null, rows are returned as Dictionary&lt;string, object&gt;.</param>
    /// <returns>A list of T, each representing a row from the query result.</returns>
    public async Task<List<T>> FetchWithJoinsAsync<T>(
        string baseTable,
        string selectClause,
        List<string> joins,
        Dictionary<string, object>? filters = null,
        Func<IDataRecord, T>? map = null)
    {
        var results = new List<T>();

        // 1. Build SELECT clause and FROM base table
        string sql = $"SELECT {selectClause} FROM {baseTable}";

        // 2. Append JOINs
        if (joins.Any())
            sql += " " + string.Join(" ", joins);

        // 3. Append WHERE clause if filters are provided
        if (filters != null && filters.Any())
        {
            var whereClause = string.Join(" AND ", filters.Keys.Select(k => $"{k} = @{k}"));
            sql += $" WHERE {whereClause}";
        }

        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(sql, conn);

        // 4. Add parameters to prevent SQL injection and support dynamic filtering
        if (filters != null)
        {
            foreach (var (key, value) in filters)
            {
                cmd.Parameters.AddWithValue("@" + key, value ?? DBNull.Value);
            }
        }

        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();

        // 5. Read rows and map results
        while (await reader.ReadAsync())
        {
            if (map != null)
            {
                // Use provided mapping function to convert each row into T
                results.Add(map(reader));
            }
            else
            {
                // Fallback: return each row as Dictionary<string, object>
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string columnName = reader.GetName(i);
                    object value = await reader.IsDBNullAsync(i) ? null! : reader.GetValue(i);
                    row[columnName] = value;
                }

                results.Add((T)(object)row);
            }
        }

        return results;
    }
}

