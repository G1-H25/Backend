var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    connectionString = Environment.GetEnvironmentVariable("SQLAZURECONNSTR_DefaultConnection")
                       ?? Environment.GetEnvironmentVariable("SQLCONNSTR_DefaultConnection")
                       ?? Environment.GetEnvironmentVariable("CUSTOMCONNSTR_DefaultConnection");
}

if (string.IsNullOrEmpty(connectionString))
{
    // Log a warning instead of throwing
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

// Configure the HTTP request pipeline.
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
