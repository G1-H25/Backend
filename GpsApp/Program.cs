var builder = WebApplication.CreateBuilder(args);

// Resolve connection string
var connectionString = builder.Configuration.GetResolvedConnectionString("DefaultConnection");


// Register services
builder.Services
    .AddApplicationServices()
    .AddSwaggerDocumentation()
    .AddInfrastructureServices(connectionString);

// Build the app
var app = builder.Build();

// error handleing
var logger = app.Services.GetRequiredService<ILogger<Program>>();
if (string.IsNullOrEmpty(connectionString))
{
    logger.LogWarning("Missing 'DefaultConnection' connection string. Database functionality will be disabled.");
    Console.WriteLine("Warning: Missing 'DefaultConnection' connection string. Database functionality will be disabled.");
}

// Configure middleware
app.UseApplicationMiddleware();

// Run the app
app.Run();
