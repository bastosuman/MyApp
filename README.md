# Financial Services Platform

A full-stack financial services application built with .NET 8 (C# and VB.NET) and React, demonstrating multi-language interoperability and modern software architecture.

## 🏗️ Architecture

### Backend Projects
- **MyApp.Core** (C#) - Domain entities and interfaces
- **MyApp.Data** (C#) - Entity Framework Core data access layer
- **MyApp.Services.VB** (VB.NET) - Business logic services (demonstrates C#/VB.NET interoperability)
- **MyApp** (C#) - Main REST API application
- **MyApp.Tests** (C#) - Unit and integration tests

### Frontend
- **BankUI** (React + TypeScript + Vite) - Modern web frontend

## 🗄️ Database Setup

### Prerequisites
- SQL Server installed and running
- .NET 8 SDK installed
- Entity Framework Core tools installed globally

### Installation Steps

1. **Install EF Core Tools** (if not already installed):
   ```powershell
   dotnet tool install --global dotnet-ef
   ```

2. **Configure Connection String**:
   Update `MyApp/appsettings.json` with your SQL Server instance:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER\\INSTANCE;Database=MyAppFinancial;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }
   ```

3. **Create and Apply Migrations**:
   ```powershell
   # Navigate to solution root
   cd MyApp
   
   # Create initial migration
   dotnet ef migrations add InitialCreate --project .\MyApp.Data\MyApp.Data.csproj --startup-project .\MyApp\MyApp.csproj
   
   # Apply migration to create database
   dotnet ef database update --project .\MyApp.Data\MyApp.Data.csproj --startup-project .\MyApp\MyApp.csproj
   ```

4. **Verify Database**:
   - Database `MyAppFinancial` will be created automatically
   - Seed data will populate on first application startup
   - Verify in SQL Server Management Studio (SSMS) or Azure Data Studio

### Database Schema

#### Tables
- **Accounts** - User account information
  - Primary Key: `Id` (int, identity)
  - Unique Index: `AccountNumber`
  - Index: `Email`
  
- **Products** - Financial products catalog
  - Primary Key: `Id` (int, identity)
  - Fields: Name, ProductType, MinAmount, MaxAmount, InterestRate, TermMonths, IsActive, Description

- **Transactions** - Financial transactions
  - Primary Key: `Id` (int, identity)
  - Foreign Key: `AccountId` → `Accounts.Id`
  - Fields: TransactionType, Amount, TransactionDate, Description, Status

- **Applications** - Loan/credit applications
  - Primary Key: `Id` (int, identity)
  - Foreign Key: `AccountId` → `Accounts.Id`
  - Fields: ApplicationType, RequestedAmount, ApprovedAmount, InterestRate, TermMonths, Status, ApplicationDate, DecisionDate, Notes

### Seed Data

The database is automatically seeded on first startup with:
- **4 Products**:
  - Personal Loan (5.5% rate, $1,000 - $50,000)
  - Home Loan (4.2% rate, $50,000 - $500,000)
  - Credit Card (18.9% rate, $500 - $10,000)
  - Savings Account (2.5% rate)

- **3 Sample Accounts**:
  - ACC001 - John Doe
  - ACC002 - Jane Smith
  - ACC003 - Robert Johnson

## 🚀 Running the Application

### Backend API
```powershell
cd MyApp
dotnet run
```
API will be available at: `https://localhost:5001` or `http://localhost:5000`
Swagger UI: `https://localhost:5001/swagger`

### Frontend (BankUI)
```powershell
cd BankUI
npm install
npm run dev
```
Frontend will be available at: `http://localhost:5173`

## 📚 API Documentation

Once the application is running, access Swagger documentation at:
- Development: `http://localhost:5000/swagger` or `https://localhost:5001/swagger`

## 🧪 Testing

```powershell
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 🔧 Technologies

- **Backend**: .NET 8, ASP.NET Core Web API, Entity Framework Core
- **Database**: SQL Server
- **Languages**: C# and VB.NET (interoperability demonstration)
- **Frontend**: React, TypeScript, Vite
- **Testing**: xUnit, Coverlet
- **Code Quality**: SonarCloud integration

## 📝 Notes

- Connection string uses Windows Authentication (`Trusted_Connection=True`)
- Database name: `MyAppFinancial`
- Seed data initializer runs automatically on application startup (only if tables are empty)
- Use SQL Server Management Studio (SSMS) or Azure Data Studio to verify database

## 🔗 Related Documentation

- [Issue 1: Database Setup & Migrations](.github/issues/issue-1-database-setup.md)
- [Financial Platform Setup Guide](FINANCIAL-PLATFORM-SETUP.md)
