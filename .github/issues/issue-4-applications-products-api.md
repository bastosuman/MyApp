# Issue 4: Applications & Products Management API

**Labels:** `enhancement`, `api`, `backend`

---

## 🎯 Goal
Implement full CRUD API endpoints for managing financial applications (loans, credit) and financial products with proper business logic.

## ✅ Prerequisites
- [x] Database setup complete (Issue #1)
- [x] AccountsController complete (Issue #2)
- [x] Application and Product entities defined
- [x] VB.NET CalculationService available

## 📋 Tasks - ApplicationsController

### Step 1: Create ApplicationsController
- [ ] Create `MyApp/Controllers/ApplicationsController.cs`
- [ ] Inject `ApplicationDbContext` and `ICalculationService`
- [ ] Add route and API controller attributes

### Step 2: Implement GET Endpoints
- [ ] `GET /api/applications`
  - [ ] Return all applications with pagination
  - [ ] Filter by account ID, type, status
  - [ ] Include account and product information
  - [ ] Sort by application date (newest first)

- [ ] `GET /api/applications/{id}`
  - [ ] Get application by ID with full details
  - [ ] Include account and product information
  - [ ] Return 404 if not found

- [ ] `GET /api/accounts/{accountId}/applications`
  - [ ] Get all applications for specific account
  - [ ] Validate account exists

### Step 3: Implement POST Endpoint
- [ ] `POST /api/applications`
  - [ ] Create new application
  - [ ] Validate account exists
  - [ ] Validate product exists and is active
  - [ ] Validate requested amount is within product limits
  - [ ] Use VB.NET CalculationService to calculate interest rate
  - [ ] Use VB.NET CalculationService to calculate monthly payment
  - [ ] Set status to "Pending"
  - [ ] Set ApplicationDate to current date
  - [ ] Return 201 Created with application details

### Step 4: Implement Approval/Rejection
- [ ] `PUT /api/applications/{id}/approve`
  - [ ] Approve application
  - [ ] Validate application exists and is pending
  - [ ] Set ApprovedAmount (may differ from RequestedAmount)
  - [ ] Set status to "Approved"
  - [ ] Set DecisionDate to current date
  - [ ] Recalculate monthly payment if approved amount differs
  - [ ] Return 200 OK with approved application

- [ ] `PUT /api/applications/{id}/reject`
  - [ ] Reject application
  - [ ] Validate application exists and is pending
  - [ ] Set status to "Rejected"
  - [ ] Set DecisionDate to current date
  - [ ] Add rejection notes
  - [ ] Return 200 OK with rejected application

### Step 5: Implement Update Endpoint
- [ ] `PUT /api/applications/{id}`
  - [ ] Update application (only if pending)
  - [ ] Allow updating RequestedAmount, TermMonths
  - [ ] Recalculate interest and monthly payment
  - [ ] Prevent updates if status is not "Pending"

## 📋 Tasks - ProductsController

### Step 6: Create ProductsController
- [ ] Create `MyApp/Controllers/ProductsController.cs`
- [ ] Inject `ApplicationDbContext`
- [ ] Add route and API controller attributes

### Step 7: Implement GET Endpoints
- [ ] `GET /api/products`
  - [ ] Return all products
  - [ ] Filter by product type, active status
  - [ ] Return only active products by default
  - [ ] Support pagination

- [ ] `GET /api/products/{id}`
  - [ ] Get product by ID
  - [ ] Return 404 if not found

- [ ] `GET /api/products?type={type}`
  - [ ] Filter products by type
  - [ ] Valid types: "Loan", "CreditCard", "SavingsAccount"

### Step 8: Implement POST Endpoint
- [ ] `POST /api/products`
  - [ ] Create new product (admin function)
  - [ ] Validate required fields
  - [ ] Validate MinAmount < MaxAmount
  - [ ] Validate MinTermMonths < MaxTermMonths
  - [ ] Set IsActive to true by default
  - [ ] Return 201 Created

### Step 9: Implement PUT Endpoint
- [ ] `PUT /api/products/{id}`
  - [ ] Update existing product
  - [ ] Allow updating interest rates, limits, terms
  - [ ] Validate product exists
  - [ ] Return 200 OK

### Step 10: Implement DELETE Endpoint
- [ ] `DELETE /api/products/{id}`
  - [ ] Soft delete (set IsActive = false)
  - [ ] Check for existing applications using this product
  - [ ] Prevent deletion if product has active applications
  - [ ] Return 204 NoContent

### Step 11: Create DTOs
- [ ] Create DTOs for Application (create, update, response)
- [ ] Create DTOs for Product (create, update, response)
- [ ] Include calculated fields in Application response (monthlyPayment, totalInterest)

### Step 12: Add Business Logic
- [ ] Integration with VB.NET CalculationService for loan calculations
- [ ] Validation for application amount within product limits
- [ ] Validation for term within product range
- [ ] Automatic interest rate assignment based on product

## 📝 API Endpoints Specification

### POST /api/applications
**Request Body:**
```json
{
  "accountId": 1,
  "applicationType": "Loan",
  "requestedAmount": 25000.00,
  "termMonths": 36,
  "productId": 1
}
```

**Response:**
```json
{
  "id": 1,
  "accountId": 1,
  "applicationType": "Loan",
  "requestedAmount": 25000.00,
  "interestRate": 5.5,
  "termMonths": 36,
  "status": "Pending",
  "applicationDate": "2025-11-03T10:00:00Z",
  "monthlyPayment": 754.84,
  "totalInterest": 2174.24
}
```

### GET /api/products
**Response:**
```json
[
  {
    "id": 1,
    "name": "Personal Loan",
    "productType": "Loan",
    "minAmount": 1000.00,
    "maxAmount": 50000.00,
    "interestRate": 5.5,
    "minTermMonths": 12,
    "maxTermMonths": 60,
    "isActive": true,
    "description": "Flexible personal loan with competitive rates"
  }
]
```

## ✅ Acceptance Criteria
- [ ] ApplicationsController with all endpoints implemented
- [ ] ProductsController with all endpoints implemented
- [ ] Integration with VB.NET CalculationService working
- [ ] Business logic validation implemented
- [ ] Input validation working
- [ ] Error handling implemented
- [ ] Swagger documentation complete
- [ ] Unit tests written (80%+ coverage)

## 🔗 Related Files
- `MyApp.Core/Entities/Application.cs`
- `MyApp.Core/Entities/Product.cs`
- `MyApp.Core/Interfaces/ICalculationService.cs`
- `MyApp.Services.VB/CalculationService.vb`
- `MyApp/Controllers/ApplicationsController.cs` (to be created)
- `MyApp/Controllers/ProductsController.cs` (to be created)


