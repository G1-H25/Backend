var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

var connectionString = config.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    connectionString = Environment.GetEnvironmentVariable("SQLAZURECONNSTR_DefaultConnection")
                       ?? Environment.GetEnvironmentVariable("SQLCONNSTR_DefaultConnection")
                       ?? Environment.GetEnvironmentVariable("CUSTOMCONNSTR_DefaultConnection");
}

                        var conn1 = Environment.GetEnvironmentVariable("SQLAZURECONNSTR_DefaultConnection");
                        var conn2 = Environment.GetEnvironmentVariable("SQLCONNSTR_DefaultConnection");
                        var conn3 = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_DefaultConnection");

                        Console.WriteLine($"SQLAZURECONNSTR_DefaultConnection: {conn1}");
                        Console.WriteLine($"SQLCONNSTR_DefaultConnection: {conn2}");
                        Console.WriteLine($"CUSTOMCONNSTR_DefaultConnection: {conn3}");

if (string.IsNullOrEmpty(connectionString))
{
    // Console fallback for early warning
    Console.WriteLine("Warning: Missing 'DefaultConnection' connection string. Database functionality will be disabled.");
    connectionString = null;
}

// Add services to the container.
builder.Services.AddControllers();

if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddSingleton<SqlInsert>(_ => new SqlInsert(connectionString));
    builder.Services.AddSingleton<SqlGet>(_ => new SqlGet(connectionString));
}

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

if (string.IsNullOrEmpty(connectionString))
{
    logger.LogWarning("Missing 'DefaultConnection' connection string. Database functionality will be disabled.");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
