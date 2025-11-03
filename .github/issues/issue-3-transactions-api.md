# Issue 3: Transaction Management API

**Labels:** `enhancement`, `api`, `backend`

---

## 🎯 Goal
Implement full CRUD API endpoints for managing financial transactions with account relationship handling.

## ✅ Prerequisites
- [x] Database setup complete (Issue #1)
- [x] AccountsController complete (Issue #2)
- [x] Transaction entity defined

## 📋 Tasks

### Step 1: Create TransactionsController
- [ ] Create `MyApp/Controllers/TransactionsController.cs`
- [ ] Add constructor with `ApplicationDbContext` dependency injection
- [ ] Add route attribute: `[Route("api/[controller]")]`
- [ ] Add `[ApiController]` attribute

### Step 2: Implement GET Endpoints
- [ ] `GET /api/transactions`
  - [ ] Return all transactions
  - [ ] Support pagination (page, pageSize)
  - [ ] Filter by account ID, type, status, date range
  - [ ] Include account information
  - [ ] Sort by transaction date (newest first)
  - [ ] Return 200 OK with transactions list

- [ ] `GET /api/transactions/{id}`
  - [ ] Get transaction by ID
  - [ ] Include account information
  - [ ] Return 404 if not found
  - [ ] Return 200 OK with transaction details

- [ ] `GET /api/accounts/{accountId}/transactions`
  - [ ] Get all transactions for a specific account
  - [ ] Validate account exists
  - [ ] Support filtering and pagination
  - [ ] Return 200 OK with account's transactions
  - [ ] Return 404 if account not found

### Step 3: Implement POST Endpoint
- [ ] `POST /api/transactions`
  - [ ] Create new transaction
  - [ ] Validate account exists
  - [ ] Validate required fields (AccountId, Amount, TransactionType)
  - [ ] Set TransactionDate to current date/time
  - [ ] Set Status to "Pending" by default
  - [ ] Validate amount > 0
  - [ ] Return 201 Created with new transaction
  - [ ] Return 400 BadRequest for validation errors
  - [ ] Return 404 if account not found

### Step 4: Implement PUT Endpoint
- [ ] `PUT /api/transactions/{id}`
  - [ ] Update existing transaction
  - [ ] Validate transaction exists
  - [ ] Allow updating Description and Status
  - [ ] Prevent updating Amount after completion
  - [ ] Validate status transitions (Pending → Completed/Failed)
  - [ ] Return 200 OK with updated transaction
  - [ ] Return 404 if not found
  - [ ] Return 400 for invalid updates

### Step 5: Implement DELETE Endpoint
- [ ] `DELETE /api/transactions/{id}`
  - [ ] Delete transaction (hard delete - only if status is Pending)
  - [ ] Validate transaction exists
  - [ ] Only allow deletion if status is "Pending"
  - [ ] Return 204 NoContent on success
  - [ ] Return 404 if not found
  - [ ] Return 400 if transaction cannot be deleted

### Step 6: Create DTOs
- [ ] Create `Dtos/TransactionDto.cs` for responses
- [ ] Create `Dtos/CreateTransactionRequest.cs` for creation
- [ ] Create `Dtos/UpdateTransactionRequest.cs` for updates
- [ ] Map entities to DTOs

### Step 7: Add Business Logic
- [ ] Validate transaction types: "Deposit", "Withdrawal", "Payment", "Transfer"
- [ ] Validate status values: "Pending", "Completed", "Failed"
- [ ] Add transaction amount validation based on type
- [ ] Implement transaction history aggregation

### Step 8: Add Validation & Error Handling
- [ ] Add Data Annotations to request DTOs
- [ ] Validate transaction type enum
- [ ] Validate amount format and range
- [ ] Handle database exceptions
- [ ] Return appropriate HTTP status codes

## 📝 API Endpoints Specification

### GET /api/transactions?accountId={id}&type={type}&status={status}
**Query Parameters:**
- `accountId` (optional): Filter by account
- `type` (optional): Filter by transaction type
- `status` (optional): Filter by status
- `startDate` (optional): Filter from date
- `endDate` (optional): Filter to date
- `page` (optional): Page number
- `pageSize` (optional): Items per page

**Response:**
```json
[
  {
    "id": 1,
    "accountId": 1,
    "transactionType": "Deposit",
    "amount": 1000.00,
    "transactionDate": "2025-11-03T10:00:00Z",
    "description": "Initial deposit",
    "status": "Completed",
    "account": {
      "id": 1,
      "accountNumber": "ACC001",
      "firstName": "John",
      "lastName": "Doe"
    }
  }
]
```

### POST /api/transactions
**Request Body:**
```json
{
  "accountId": 1,
  "transactionType": "Deposit",
  "amount": 500.00,
  "description": "Monthly salary deposit"
}
```

**Response:**
```json
{
  "id": 2,
  "accountId": 1,
  "transactionType": "Deposit",
  "amount": 500.00,
  "transactionDate": "2025-11-03T12:00:00Z",
  "description": "Monthly salary deposit",
  "status": "Pending"
}
```

## ✅ Acceptance Criteria
- [ ] All CRUD endpoints implemented
- [ ] Account relationship validation working
- [ ] Filtering and pagination working
- [ ] Input validation working
- [ ] Error handling implemented
- [ ] Swagger documentation complete
- [ ] Unit tests written (80%+ coverage)

## 🔗 Related Files
- `MyApp.Core/Entities/Transaction.cs`
- `MyApp.Core/Entities/Account.cs`
- `MyApp/Controllers/TransactionsController.cs` (to be created)


