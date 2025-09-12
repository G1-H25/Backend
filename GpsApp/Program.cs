var builder = WebApplication.CreateBuilder(args);

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
