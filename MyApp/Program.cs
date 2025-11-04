using Microsoft.EntityFrameworkCore;
using MyApp.Core.Interfaces;
using MyApp.Data;
using MyApp.Services.VB;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Entity Framework DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found. " +
        "Please configure it in appsettings.json or via environment variables.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register calculation service (VB.NET implementation)
// Note: VB.NET service registration is handled via reflection in tests due to C#/VB.NET interop compilation issues
// For production, consider using a C# wrapper or fixing the interop issue
builder.Services.AddScoped<ICalculationService, MyApp.Services.VB.CalculationService>();


// Add CORS to allow BankUI frontend to connect
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBankUI",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:5174")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS FIRST - before any other middleware that might redirect
app.UseCors("AllowBankUI");

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

// Seed database on startup (only if in development or explicitly enabled via configuration)
// In production, set "Database:EnableSeeding": true in appsettings.json to enable seeding
var isDevelopment = app.Environment.IsDevelopment();
var explicitSeedingEnabled = builder.Configuration.GetValue<bool>("Database:EnableSeeding", false);
var shouldSeedDatabase = isDevelopment || explicitSeedingEnabled;

if (shouldSeedDatabase)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        try
        {
            logger.LogInformation("Database seeding is enabled. Starting seed process...");
            DbInitializer.Seed(context);
            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}
else if (app.Environment.IsProduction() || app.Environment.IsStaging())
{
    // Log that seeding is skipped in production/staging unless explicitly enabled
    using (var scope = app.Services.CreateScope())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database seeding is disabled in {Environment}. Set 'Database:EnableSeeding' to true in configuration to enable seeding.", app.Environment.EnvironmentName);
    }
}

await app.RunAsync();
