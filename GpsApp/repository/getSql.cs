using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SqlGet
{
    private readonly string _connectionString;

    public SqlGet(string connectionString)
    {
        _connectionString = connectionString
            ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Fetches a single row from the specified table based on filter conditions.
    /// </summary>
    /// <param name="tableName">The name of the table to query.</param>
    /// <param name="filters">A dictionary of column-value pairs used as WHERE conditions.</param>
    /// <param name="columns">Optional: list of columns to select; defaults to all (*)</param>
    /// <returns>A dictionary of column names and their values, or null if no match.</returns>
    public async Task<Dictionary<string, object>?> FetchAsync(
        string tableName,
        Dictionary<string, object> filters,
        IEnumerable<string>? columns = null)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name must be provided.", nameof(tableName));

        if (filters == null || filters.Count == 0)
            throw new ArgumentException("At least one filter condition must be provided.", nameof(filters));

        // Prepare the SELECT clause
        string selectedColumns = (columns != null && columns.Any())
            ? string.Join(", ", columns)
            : "*";

        // Prepare the WHERE clause with parameter placeholders
        string whereClause = string.Join(" AND ", filters.Keys.Select(k => $"{k} = @{k}"));

        // Final SQL query
        string sql = $"SELECT {selectedColumns} FROM dbo.{tableName} WHERE {whereClause}";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);

        // Add parameters to the command
        foreach (var (key, value) in filters)
        {
            command.Parameters.AddWithValue("@" + key, value ?? DBNull.Value);
        }

        await connection.OpenAsync();

        await using var reader = await command.ExecuteReaderAsync();

        // Read the first row if it exists
        if (await reader.ReadAsync())
        {
            var result = new Dictionary<string, object>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);
                object value = await reader.IsDBNullAsync(i) ? null! : reader.GetValue(i);
                result[columnName] = value;
            }

            return result;
        }

        // No rows matched the query
        return null;
    }
}
