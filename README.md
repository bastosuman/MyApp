# MyApp Financial Services

A .NET 8.0 financial services application with React frontend.

## Project Structure

```
MyApp/
├── MyApp/              # Main API project
├── MyApp.Core/         # Entity models and domain logic
├── MyApp.Data/         # Data access layer (Entity Framework)
├── MyApp.Tests/        # Unit tests
└── BankUI/             # React frontend
```

## Database Setup

### Prerequisites

1. **SQL Server** - Ensure SQL Server is running on localhost
2. **Entity Framework Tools** - Install globally:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

### Connection String

The connection string is configured in `MyApp/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyAppFinancial;Trusted_Connection=True;"
  }
}
```

**Security Note:** The connection string uses Windows Authentication (`Trusted_Connection=True`) and does NOT include `TrustServerCertificate=True` for production security. For local development migrations, the design-time factory in `MyApp.Data` uses `TrustServerCertificate=True` only for EF tooling.

### Initial Setup

1. **Verify Configuration**
   - Ensure `appsettings.json` contains the correct connection string
   - Verify SQL Server is accessible on localhost

2. **Create Initial Migration** (if not already created)
   ```bash
   cd MyApp.Data
   dotnet ef migrations add InitialCreate
   ```

3. **Apply Migration to Database**
   ```bash
   cd MyApp.Data
   dotnet ef database update
   ```

4. **Seed Data**
   - Seed data is automatically initialized when the application starts in Development mode
   - The `DbInitializer.Initialize()` method is called from `Program.cs`
   - Seed data includes:
     - 4 financial products (Personal Loan, Home Loan, Credit Card, Savings Account)
     - 3 sample accounts
     - 4 sample transactions

### Database Schema

See [DATABASE_SCHEMA.md](./DATABASE_SCHEMA.md) for complete database schema documentation.

### Tables Created

- **Accounts** - Customer account information
- **Transactions** - Financial transaction records
- **Products** - Available financial products
- **Applications** - Product applications
- **__EFMigrationsHistory** - Entity Framework migration history

### Verifying Database Setup

1. **Check Tables Exist:**
   ```sql
   SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
   WHERE TABLE_TYPE = 'BASE TABLE'
   ORDER BY TABLE_NAME;
   ```

2. **Check Seed Data:**
   ```sql
   SELECT COUNT(*) FROM Products;  -- Should return 4
   SELECT COUNT(*) FROM Accounts;  -- Should return 3
   SELECT COUNT(*) FROM Transactions; -- Should return 4
   ```

3. **View Products:**
   ```sql
   SELECT Name, ProductType, InterestRate, MinAmount, MaxAmount 
   FROM Products 
   ORDER BY Name;
   ```

4. **View Accounts:**
   ```sql
   SELECT AccountNumber, AccountHolderName, Balance, AccountType 
   FROM Accounts 
   ORDER BY AccountNumber;
   ```

## Running the Application

### Backend (MyApp)

```bash
cd MyApp
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5250`
- HTTPS: `https://localhost:7059`
- Swagger UI: `https://localhost:7059/swagger` (in development)

### Frontend (BankUI)

```bash
cd BankUI
npm install
npm run dev
```

The frontend will run on `http://localhost:3000`

## Development Notes

- Database seed data is initialized automatically on application startup in Development mode
- The `DbInitializer` checks if data already exists before seeding to prevent duplicates
- All dates are stored in UTC format
- Entity Framework migrations are stored in `MyApp.Data/Migrations/`

## Testing

Run unit tests:
```bash
dotnet test
```

See [MyApp.Tests/TEST_SUMMARY.md](./MyApp.Tests/TEST_SUMMARY.md) for test documentation.

## Security

- Connection strings use Windows Authentication for secure access
- `TrustServerCertificate=True` is NOT used in production connection strings
- SSL certificate validation is enforced for database connections
