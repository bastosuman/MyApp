using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

builder.Services.AddDbContext<FinancialDbContext>(options =>
    options.UseSqlServer(connectionString));

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

// Add global exception handling
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

// Initialize database with seed data (only in development)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<FinancialDbContext>();
        DbInitializer.Initialize(context);
    }
}

await app.RunAsync();
