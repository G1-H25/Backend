// Extensions/GetConnectionString.cs
public static class GetConnectionString
{
    public static string GetResolvedConnectionString(this IConfiguration config, string name)
    {
        var connectionString = config.GetConnectionString(name);

        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = Environment.GetEnvironmentVariable($"SQLAZURECONNSTR_{name}")
                               ?? Environment.GetEnvironmentVariable($"SQLCONNSTR_{name}")
                               ?? Environment.GetEnvironmentVariable($"CUSTOMCONNSTR_{name}");
        }


        return connectionString;
    }
}
