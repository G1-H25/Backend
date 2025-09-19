using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

var connectionString = config.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = Environment.GetEnvironmentVariable("SQLAZURECONNSTR_DefaultConnection")
                       ?? Environment.GetEnvironmentVariable("SQLCONNSTR_DefaultConnection")
                       ?? Environment.GetEnvironmentVariable("CUSTOMCONNSTR_DefaultConnection");
}

if (string.IsNullOrEmpty(connectionString))
{
    // Console fallback for early warning
    Console.WriteLine("Warning: Missing 'DefaultConnection' connection string. Database functionality will be disabled.");
    connectionString = null;
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    options.IncludeXmlComments(xmlPath);
});

// Register SqlInsert and SqlGet only if connection string is available
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

// Enable Swagger UI only in dev or staging 
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
