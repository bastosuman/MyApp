# Issue 2: Account Management API

**Labels:** `enhancement`, `api`, `backend`

---

## 🎯 Goal
Implement full CRUD API endpoints for managing user accounts with proper validation and error handling.

## ✅ Prerequisites
- [x] Database setup complete (Issue #1)
- [x] DbContext configured
- [x] Account entity defined

## 📋 Tasks

### Step 1: Create AccountsController
- [ ] Create `MyApp/Controllers/AccountsController.cs`
- [ ] Add constructor with `ApplicationDbContext` dependency injection
- [ ] Add route attribute: `[Route("api/[controller]")]`
- [ ] Add `[ApiController]` attribute

### Step 2: Implement GET Endpoints
- [ ] `GET /api/accounts`
  - [ ] Return all accounts (with pagination support)
  - [ ] Include related transactions and applications
  - [ ] Add filtering by status, email, or account number
  - [ ] Return 200 OK with accounts list
  
- [ ] `GET /api/accounts/{id}`
  - [ ] Get account by ID
  - [ ] Include related transactions and applications
  - [ ] Return 404 if not found
  - [ ] Return 200 OK with account details

### Step 3: Implement POST Endpoint
- [ ] `POST /api/accounts`
  - [ ] Create new account
  - [ ] Generate unique account number
  - [ ] Validate required fields (FirstName, LastName, Email)
  - [ ] Check for duplicate email
  - [ ] Set DateCreated to current date
  - [ ] Set IsActive to true by default
  - [ ] Return 201 Created with new account
  - [ ] Return 400 BadRequest for validation errors

### Step 4: Implement PUT Endpoint
- [ ] `PUT /api/accounts/{id}`
  - [ ] Update existing account
  - [ ] Validate account exists
  - [ ] Update allowed fields (FirstName, LastName, Phone)
  - [ ] Prevent updating AccountNumber
  - [ ] Return 200 OK with updated account
  - [ ] Return 404 if not found
  - [ ] Return 400 for validation errors

### Step 5: Implement DELETE Endpoint
- [ ] `DELETE /api/accounts/{id}`
  - [ ] Soft delete (set IsActive = false)
  - [ ] Validate account exists
  - [ ] Check if account has active transactions/applications
  - [ ] Return 204 NoContent on success
  - [ ] Return 404 if not found
  - [ ] Return 400 if account cannot be deleted (has dependencies)

### Step 6: Create DTOs
- [ ] Create `Dtos/AccountDto.cs` for responses
- [ ] Create `Dtos/CreateAccountRequest.cs` for creation
- [ ] Create `Dtos/UpdateAccountRequest.cs` for updates
- [ ] Map entities to DTOs (use AutoMapper or manual mapping)

### Step 7: Add Validation
- [ ] Add Data Annotations to request DTOs
- [ ] Validate email format
- [ ] Validate phone number format
- [ ] Validate required fields
- [ ] Return appropriate error messages

### Step 8: Add Error Handling
- [ ] Handle database exceptions
- [ ] Return appropriate HTTP status codes
- [ ] Return consistent error response format
- [ ] Log errors

## 📝 API Endpoints Specification

### GET /api/accounts
**Query Parameters:**
- `page` (optional): Page number
- `pageSize` (optional): Items per page
- `isActive` (optional): Filter by active status
- `email` (optional): Search by email

**Response:**
```json
[
  {
    "id": 1,
    "accountNumber": "ACC001",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "phone": "123-456-7890",
    "dateCreated": "2025-11-03T10:00:00Z",
    "isActive": true
  }
]
```

### POST /api/accounts
**Request Body:**
```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@example.com",
  "phone": "987-654-3210"
}
```

**Response:**
```json
{
  "id": 2,
  "accountNumber": "ACC002",
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@example.com",
  "phone": "987-654-3210",
  "dateCreated": "2025-11-03T12:00:00Z",
  "isActive": true
}
```

## ✅ Acceptance Criteria
- [ ] All 5 CRUD endpoints implemented
- [ ] Input validation working
- [ ] Error handling implemented
- [ ] Swagger documentation complete
- [ ] Unit tests written (80%+ coverage)
- [ ] Manual testing completed via Swagger UI

## 🔗 Related Files
- `MyApp.Core/Entities/Account.cs`
- `MyApp.Data/ApplicationDbContext.cs`
- `MyApp/Controllers/AccountsController.cs` (to be created)


