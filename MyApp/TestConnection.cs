using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace MyApp
{
    public static class TestConnection
    {
        public static void TestDatabaseConnection()
        {
            try
            {
                // Build configuration to read from appsettings.json
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");
                
                Console.WriteLine("=== Testing Database Connection ===");
                Console.WriteLine($"Connection String: {connectionString}");
                Console.WriteLine();

                using (var connection = new SqlConnection(connectionString))
                {
                    Console.WriteLine("Attempting to connect...");
                    connection.Open();
                    Console.WriteLine("✓ Connection successful!");
                    
                    // Test query
                    var command = new SqlCommand("SELECT DB_NAME() AS CurrentDatabase, @@VERSION AS SqlVersion", connection);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var currentDb = reader["CurrentDatabase"]?.ToString() ?? "master";
                            var version = reader["SqlVersion"]?.ToString()?.Split('\n')[0] ?? "Unknown";
                            Console.WriteLine($"Current Database: {currentDb}");
                            Console.WriteLine($"SQL Server Version: {version}");
                        }
                    }
                    
                    // Check if MyAppFinancial database exists
                    var dbCheckCommand = new SqlCommand(
                        "SELECT name FROM sys.databases WHERE name = 'MyAppFinancial'", 
                        connection);
                    var dbExists = dbCheckCommand.ExecuteScalar() != null;
                    
                    Console.WriteLine($"MyAppFinancial database exists: {(dbExists ? "✓ Yes" : "✗ No")}");
                }
                
                Console.WriteLine();
                Console.WriteLine("=== Connection Test Complete ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Connection failed!");
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }
    }
}

