using Microsoft.EntityFrameworkCore;
// using MyApp.Core.Interfaces;
using MyApp.Data;
// using MyApp.Services.VB;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Entity Framework DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=SUMAN\\MSSQLSERVER01;Database=MyAppFinancial;Trusted_Connection=True;TrustServerCertificate=True;"));

// Register VB.NET calculation service (demonstrates C#/VB.NET interoperability)
// Temporarily commented to allow migrations - will uncomment after database setup
// builder.Services.AddScoped<ICalculationService, CalculationService>();

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

// Seed database on startup (only if in development or explicitly enabled)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    try
    {
        DbInitializer.Seed(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

await app.RunAsync();
