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

builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    Console.WriteLine("[DEBUG] XML Path: " + xmlPath); // Debug print to verify path
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
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
