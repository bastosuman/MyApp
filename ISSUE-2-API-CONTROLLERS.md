# Issue #2: Implement REST API Controllers for Financial Services

## Description
Create RESTful API controllers to expose financial services functionality through HTTP endpoints. This will enable the frontend (BankUI) to interact with the financial data (Accounts, Products, Transactions, Applications).

## Current Status
- ✅ Database layer is complete (MyApp.Data)
- ✅ Entity models are defined (MyApp.Core)
- ✅ Database is seeded with sample data
- ❌ No API controllers exist
- ❌ No endpoints to retrieve/create/update financial data

## Required Controllers

### 1. AccountsController
**Endpoints:**
- `GET /api/accounts` - Get all accounts
- `GET /api/accounts/{id}` - Get account by ID
- `POST /api/accounts` - Create new account
- `PUT /api/accounts/{id}` - Update account
- `GET /api/accounts/{id}/transactions` - Get transactions for an account
- `GET /api/accounts/{id}/applications` - Get applications for an account

### 2. ProductsController
**Endpoints:**
- `GET /api/products` - Get all products (filter by IsActive=true by default)
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create new product (admin only)
- `PUT /api/products/{id}` - Update product (admin only)

### 3. TransactionsController
**Endpoints:**
- `GET /api/transactions` - Get all transactions (with pagination)
- `GET /api/transactions/{id}` - Get transaction by ID
- `POST /api/transactions` - Create new transaction
- `GET /api/accounts/{accountId}/transactions` - Get transactions for specific account

### 4. ApplicationsController
**Endpoints:**
- `GET /api/applications` - Get all applications (with filtering by status)
- `GET /api/applications/{id}` - Get application by ID
- `POST /api/applications` - Submit new application
- `PUT /api/applications/{id}/status` - Update application status (approve/reject)
- `GET /api/accounts/{accountId}/applications` - Get applications for specific account

## Technical Requirements

### DTOs (Data Transfer Objects)
Create DTOs in `MyApp.Core/DTOs/` or `MyApp/Models/`:
- `AccountDto`, `CreateAccountDto`, `UpdateAccountDto`
- `ProductDto`, `CreateProductDto`
- `TransactionDto`, `CreateTransactionDto`
- `ApplicationDto`, `CreateApplicationDto`, `UpdateApplicationStatusDto`

### Validation
- Use Data Annotations or FluentValidation for input validation
- Validate required fields, data types, and business rules
- Return appropriate HTTP status codes (400 Bad Request for validation errors)

### Error Handling
- Implement global exception handling
- Return consistent error response format
- Log errors appropriately

### Response Format
- Use consistent JSON response structure
- Include proper HTTP status codes
- Return appropriate error messages

### Security Considerations
- Input validation and sanitization
- SQL injection prevention (EF Core handles this)
- Consider authentication/authorization for future implementation

## Acceptance Criteria

- [ ] All 4 controllers implemented with CRUD operations
- [ ] DTOs created for all entities
- [ ] Input validation implemented
- [ ] Error handling implemented
- [ ] Swagger documentation updated
- [ ] Unit tests for controllers (80%+ coverage)
- [ ] Integration tests for API endpoints
- [ ] API endpoints tested with Swagger UI
- [ ] Frontend can connect and fetch data from API

## Testing Requirements

### Unit Tests
- Test controller methods
- Test validation logic
- Test error handling
- Mock DbContext for controller tests

### Integration Tests
- Test API endpoints with in-memory database
- Test HTTP status codes
- Test response formats
- Test error scenarios

## Example API Response Structure

```json
{
  "success": true,
  "data": { ... },
  "message": "Operation completed successfully"
}
```

Or for errors:
```json
{
  "success": false,
  "errors": ["Validation error message"],
  "message": "Request validation failed"
}
```

## Priority
**High** - This is a critical feature to enable frontend-backend integration.

## Estimated Effort
Medium - 2-3 days

## Dependencies
- Issue #11 (Database Setup) - ✅ Completed

## Related Files
- `MyApp/Controllers/` (to be created)
- `MyApp.Core/DTOs/` (to be created)
- `MyApp.Tests/ControllerTests/` (to be created)

