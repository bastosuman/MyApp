# Database Schema Documentation

## Database: MyAppFinancial

### Overview
The MyAppFinancial database contains tables for managing financial services including accounts, transactions, applications, and products.

### Tables

#### 1. Accounts
Stores customer account information.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Identity | Primary key |
| AccountNumber | nvarchar(50) | Required, Unique Index | Unique account identifier |
| AccountHolderName | nvarchar(200) | Required | Name of account holder |
| Balance | decimal(18,2) | Required | Current account balance |
| AccountType | nvarchar(50) | Required | Type of account (Savings, Checking, etc.) |
| CreatedDate | datetime2 | Required | Account creation date |
| IsActive | bit | Required | Whether account is active |

**Relationships:**
- One-to-Many with Transactions
- One-to-Many with Applications

#### 2. Transactions
Records all financial transactions for accounts.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Identity | Primary key |
| AccountId | int | FK, Required | Reference to Accounts table |
| TransactionType | nvarchar(50) | Required | Type (Deposit, Withdrawal, Transfer, etc.) |
| Amount | decimal(18,2) | Required | Transaction amount |
| Description | nvarchar(500) | Optional | Transaction description |
| TransactionDate | datetime2 | Required | Date/time of transaction |
| Status | nvarchar(50) | Required | Status (Completed, Pending, Failed) |

**Relationships:**
- Many-to-One with Accounts (FK: AccountId)

#### 3. Products
Stores available financial products (loans, credit cards, savings accounts).

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Identity | Primary key |
| Name | nvarchar(200) | Required | Product name |
| ProductType | nvarchar(50) | Required | Type (Loan, CreditCard, SavingsAccount, etc.) |
| InterestRate | decimal(5,2) | Required | Annual interest rate percentage |
| MinAmount | decimal(18,2) | Required | Minimum amount for product |
| MaxAmount | decimal(18,2) | Required | Maximum amount for product |
| Description | nvarchar(1000) | Optional | Product description |
| IsActive | bit | Required | Whether product is currently available |
| CreatedDate | datetime2 | Required | Product creation date |

**Relationships:**
- One-to-Many with Applications

#### 4. Applications
Tracks applications for financial products.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Identity | Primary key |
| AccountId | int | FK, Required | Reference to Accounts table |
| ProductId | int | FK, Required | Reference to Products table |
| RequestedAmount | decimal(18,2) | Required | Amount requested in application |
| Status | nvarchar(50) | Required | Status (Pending, Approved, Rejected, Completed) |
| ApplicationDate | datetime2 | Required | Date application was submitted |
| DecisionDate | datetime2 | Nullable | Date decision was made |
| Notes | nvarchar(1000) | Nullable | Additional notes about application |

**Relationships:**
- Many-to-One with Accounts (FK: AccountId)
- Many-to-One with Products (FK: ProductId)

### Seed Data

#### Products
1. **Personal Loan** - 5.5% rate, $1,000 - $50,000
2. **Home Loan** - 4.2% rate, $50,000 - $500,000
3. **Credit Card** - 18.9% rate, $500 - $10,000
4. **Savings Account** - 2.5% rate, $0 - $1,000,000

#### Sample Accounts
1. **ACC001** - John Doe, Savings, $15,000.00
2. **ACC002** - Jane Smith, Checking, $8,500.50
3. **ACC003** - Bob Johnson, Savings, $25,000.00

#### Sample Transactions
- ACC001: Initial deposit $5,000, ATM withdrawal $500
- ACC002: Salary deposit $10,000, Purchase payment $1,500.50

### Indexes

- **IX_Accounts_AccountNumber** - Unique index on AccountNumber
- **IX_Transactions_AccountId** - Index on AccountId for faster lookups
- **IX_Applications_AccountId** - Index on AccountId
- **IX_Applications_ProductId** - Index on ProductId

### Entity Framework Migrations

**Initial Migration:** `20251104160102_InitialCreate`

This migration creates all tables, foreign keys, and indexes as defined in the schema.

