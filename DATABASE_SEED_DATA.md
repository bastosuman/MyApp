# Database Seed Data

This document describes the seed data that will be automatically created when the backend starts.

## Seed Data Overview

When the backend application starts, it automatically initializes the database with sample data if the database is empty.

### Products (4 items)
1. **Personal Loan**
   - Type: Loan
   - Interest Rate: 5.5%
   - Amount Range: $1,000 - $50,000
   - Description: Personal loan for individual borrowers with flexible repayment terms

2. **Home Loan**
   - Type: Loan
   - Interest Rate: 4.2%
   - Amount Range: $50,000 - $500,000
   - Description: Home mortgage loan for property purchase or refinancing

3. **Credit Card**
   - Type: CreditCard
   - Interest Rate: 18.9%
   - Amount Range: $500 - $10,000
   - Description: Credit card with competitive interest rates and rewards program

4. **Savings Account**
   - Type: SavingsAccount
   - Interest Rate: 2.5%
   - Amount Range: $0 - $1,000,000
   - Description: High-yield savings account with competitive interest rates

### Accounts (3 items)
1. **ACC001** - John Doe
   - Balance: $15,000.00
   - Type: Savings
   - Created: 6 months ago

2. **ACC002** - Jane Smith
   - Balance: $8,500.50
   - Type: Checking
   - Created: 3 months ago

3. **ACC003** - Bob Johnson
   - Balance: $25,000.00
   - Type: Savings
   - Created: 12 months ago

### Transactions (4 items)
1. Deposit of $5,000.00 to ACC001 (6 months ago)
2. Withdrawal of $500.00 from ACC001 (5 months ago)
3. Deposit of $10,000.00 to ACC002 (3 months ago)
4. Withdrawal of $1,500.50 from ACC002 (2 months ago)

### Applications (4 items)
1. **Pending** - Personal Loan application from John Doe (ACC001)
   - Requested Amount: $25,000
   - Application Date: 5 days ago
   - Notes: Application for personal loan to consolidate debt

2. **Pending** - Credit Card application from Jane Smith (ACC002)
   - Requested Amount: $5,000
   - Application Date: 2 days ago
   - Notes: Request for credit card with higher limit

3. **Approved** - Savings Account application from John Doe (ACC001)
   - Requested Amount: $10,000
   - Application Date: 1 month ago
   - Decision Date: 1 month ago (3 days later)
   - Notes: High-yield savings account application approved

4. **Rejected** - Home Loan application from Bob Johnson (ACC003)
   - Requested Amount: $600,000
   - Application Date: 2 months ago
   - Decision Date: 2 months ago (7 days later)
   - Notes: Application rejected due to insufficient credit history

## How to View Seed Data

### Option 1: Via API Endpoints
Test the API endpoints directly:
- `http://localhost:5250/api/products` - View all products
- `http://localhost:5250/api/accounts` - View all accounts
- `http://localhost:5250/api/transactions?page=1&pageSize=50` - View transactions
- `http://localhost:5250/api/applications` - View all applications

### Option 2: Via Swagger UI
1. Navigate to `http://localhost:5250/swagger`
2. Use the interactive API documentation to test endpoints

### Option 3: Via BankUI Frontend
1. Start the frontend: `cd BankUI && npm start`
2. Navigate to `http://localhost:3002` (or your frontend port)
3. Use the tabs to view:
   - **Accounts** - See all account cards
   - **Products** - See all financial products
   - **Transactions** - See transaction history
   - **Applications** - See loan/credit applications

## Resetting Seed Data

If you need to reset the seed data:
1. Delete the database `MyAppFinancial` from SQL Server
2. Restart the backend - it will automatically recreate the database and seed data

Or manually clear tables in SQL Server:
```sql
DELETE FROM Applications;
DELETE FROM Transactions;
DELETE FROM Accounts;
DELETE FROM Products;
```

Then restart the backend to reseed.

