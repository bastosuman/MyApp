# Issue 1: Database Setup & Migrations

**Labels:** `enhancement`, `database`, `backend`

---

## 🎯 Goal
Initialize the SQL Server database for the financial services platform using Entity Framework Core migrations and seed initial data.

## ✅ Prerequisites
- [x] SQL Server installed and running
- [x] Connection string configured (`Server=SUMAN\MSSQLSERVER01;Database=MyAppFinancial;Trusted_Connection=True;TrustServerCertificate=True;`)
- [x] Entity Framework Core packages installed
- [x] DbContext created with all entities

## 📋 Tasks

### Step 1: Verify Configuration
- [x] Verify connection string in `appsettings.json` points to SQL Server (SUMAN\MSSQLSERVER01)
- [x] Test database connection manually
- [x] Ensure Entity Framework tools are installed globally: `dotnet tool install --global dotnet-ef`

### Step 2: Create Initial Migration
- [x] Navigate to project directory: `cd MyApp`
- [x] Create initial migration: `dotnet ef migrations add InitialCreate --project ../MyApp.Data`
- [x] Review generated migration files in `MyApp.Data/Migrations/`
- [x] Verify migration includes all entities: Account, Transaction, Application, Product

### Step 3: Apply Migration to Database
- [x] Apply migration: `dotnet ef database update --project ../MyApp.Data`
- [x] Verify database `MyAppFinancial` was created
- [x] Verify all tables were created (Accounts, Transactions, Applications, Products)
- [x] Check table schemas match entity definitions

### Step 4: Create Seed Data
- [x] Create `Data/DbInitializer.cs` or use migration seeding
- [x] Seed sample financial products:
  - Personal Loan (5.5% rate, $1,000 - $50,000)
  - Home Loan (4.2% rate, $50,000 - $500,000)
  - Credit Card (18.9% rate, $500 - $10,000)
  - Savings Account (2.5% rate)
- [x] Seed 2-3 sample accounts for testing
- [x] Verify seed data was inserted correctly

### Step 5: Test & Verify
- [x] Run application and verify no database connection errors
- [x] Query database to confirm tables and data exist
- [x] Document database schema
- [x] Update README with database setup instructions

## 🔗 Related Files
- `MyApp.Data/ApplicationDbContext.cs`
- `MyApp/appsettings.json`
- `MyApp.Data/Migrations/`

## 📝 Notes
- Use SQL Server Management Studio (SSMS) or Azure Data Studio to verify database
- Connection string uses Windows Authentication (Trusted_Connection=True)
- Database name: `MyAppFinancial`

## ✅ Acceptance Criteria
- [x] Database `MyAppFinancial` exists
- [x] All 4 tables created (Accounts, Transactions, Applications, Products)
- [x] Seed data loaded (at least 3 products, 2 sample accounts) - **4 products, 3 accounts loaded**
- [x] Application can connect to database without errors
- [x] README updated with setup instructions


