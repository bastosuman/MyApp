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

// Register services
builder.Services.AddScoped<MyApp.Services.TransferService>();

// Add CORS to allow BankUI frontend to connect
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBankUI",
        policy =>
        {
            // Build comprehensive list of allowed origins for development
            var allowedOrigins = new List<string>();
            
            // Add common development ports (including 3002 for this setup)
            var ports = new[] { 3000, 3001, 3002, 3003, 5173, 5174, 5175, 5176, 5177, 5178, 5179, 5180 };
            foreach (var port in ports)
            {
                allowedOrigins.Add($"http://localhost:{port}");
                allowedOrigins.Add($"http://127.0.0.1:{port}");
            }

            policy.WithOrigins(allowedOrigins.ToArray())
                .AllowAnyHeader()
                .AllowAnyMethod();
                // Note: AllowCredentials() removed for development - not needed for simple API calls
                // and can cause CORS issues when combined with specific origins
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Add routing FIRST
app.UseRouting();

// Enable CORS after routing but before authorization
app.UseCors("AllowBankUI");

// Add global exception handling (after routing but before endpoints)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

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
