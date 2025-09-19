using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

// check that its using enviroment variables
Console.WriteLine("[DEBUG] ENV: " + builder.Environment.EnvironmentName);
Console.WriteLine("[DEBUG] Connection String: " + builder.Configuration.GetConnectionString("DefaultConnection"));


// Add services to the container.
builder.Services.AddControllers();

// secures connection to database through config
builder.Services.AddSingleton<SqlInsert>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Missing 'DefaultConnection' in config.");
    return new SqlInsert(connectionString);
});

builder.Services.AddSingleton<SqlGet>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Missing 'DefaultConnection' in config.");
    return new SqlGet(connectionString);
});

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

// starts swagger
if (app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
