# Issue #4: Payment Processing & Transfer System

## Description
Implement a comprehensive payment processing and transfer system that enables secure inter-account transfers, external transfers, transaction limits, and payment scheduling. This will transform the platform from basic transaction management to a fully functional banking system with proper transfer workflows, validation, and business logic.

## Current Status
- ✅ Basic transaction CRUD operations exist (Deposit, Withdrawal)
- ✅ Account balance management for deposits/withdrawals
- ✅ Transaction entity and API endpoints
- ❌ Transfer type exists but doesn't actually transfer funds between accounts
- ❌ No inter-account transfer functionality
- ❌ No external transfer capability (to other accounts by account number)
- ❌ No transaction limits (daily, monthly, per-transaction)
- ❌ No transfer scheduling/recurring payments
- ❌ No transfer validation and security checks
- ❌ No pending transfer management
- ❌ No transfer history tracking

## Business Requirements

### 1. Inter-Account Transfers
**Purpose:** Allow users to transfer funds between their own accounts.

**Features:**
- Transfer from one account to another (both owned by same user)
- Real-time balance updates for both accounts
- Transaction records created for both source and destination accounts
- Transfer confirmation and receipt
- Transfer validation (sufficient balance, account status, etc.)

**Business Rules:**
- Source account must have sufficient balance
- Both accounts must be active
- Both accounts must belong to the same account holder (for now)
- Minimum transfer amount: $1.00
- Maximum transfer amount: Based on account limits (to be configured)

### 2. External Transfers
**Purpose:** Allow users to transfer funds to other accounts by account number.

**Features:**
- Transfer to external account by account number
- Account number validation
- Transfer confirmation with recipient account details (masked)
- Pending transfer status until completion
- Transfer cancellation (if pending)

**Business Rules:**
- Source account must have sufficient balance
- Destination account must exist and be active
- Account number must be valid format
- Cannot transfer to same account
- External transfers may have different limits than inter-account transfers

### 3. Transaction Limits & Validation
**Purpose:** Implement banking-standard transaction limits and validation rules.

**Features:**
- Daily transfer limit per account
- Monthly transfer limit per account
- Per-transaction maximum limit
- Per-transaction minimum limit
- Limit tracking and enforcement
- Limit reset scheduling (daily/monthly)

**Default Limits:**
- Daily transfer limit: $10,000
- Monthly transfer limit: $50,000
- Per-transaction maximum: $5,000
- Per-transaction minimum: $1.00

**Configuration:**
- Limits should be configurable per account type
- Admin should be able to override limits for specific accounts
- Limits should be stored in database for flexibility

### 4. Transfer Scheduling
**Purpose:** Allow users to schedule future transfers and set up recurring transfers.

**Features:**
- Schedule one-time future transfer
- Create recurring transfers (daily, weekly, monthly, quarterly, annually)
- View scheduled transfers
- Edit/cancel scheduled transfers
- Automatic execution of scheduled transfers
- Notification system for scheduled transfers

**Recurring Transfer Types:**
- One-time (scheduled for specific date)
- Daily
- Weekly (specific day of week)
- Monthly (specific day of month)
- Quarterly
- Annually

### 5. Transfer Status & Tracking
**Purpose:** Provide comprehensive transfer tracking and status management.

**Features:**
- Transfer status: Pending, Processing, Completed, Failed, Cancelled
- Transfer history with detailed information
- Transfer receipts/confirmations
- Failed transfer handling and retry logic
- Transfer reversal capability (for errors)

**Status Flow:**
- Pending → Processing → Completed
- Pending → Cancelled (if cancelled before processing)
- Processing → Failed (if error occurs)
- Completed → Reversed (if reversal requested)

## Technical Requirements

### Database Schema Changes

#### New Entity: Transfer
```csharp
public class Transfer
{
    public int Id { get; set; }
    public int SourceAccountId { get; set; }
    public int? DestinationAccountId { get; set; } // Nullable for external transfers
    public string? DestinationAccountNumber { get; set; } // For external transfers
    public string TransferType { get; set; } // "Internal", "External"
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public string Status { get; set; } // "Pending", "Processing", "Completed", "Failed", "Cancelled"
    public DateTime TransferDate { get; set; }
    public DateTime? ScheduledDate { get; set; } // For scheduled transfers
    public string? RecurrencePattern { get; set; } // JSON: { "Type": "Monthly", "Day": 15 }
    public int? SourceTransactionId { get; set; }
    public int? DestinationTransactionId { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    
    // Navigation properties
    public virtual Account SourceAccount { get; set; }
    public virtual Account? DestinationAccount { get; set; }
    public virtual Transaction? SourceTransaction { get; set; }
    public virtual Transaction? DestinationTransaction { get; set; }
}
```

#### New Entity: AccountLimits
```csharp
public class AccountLimits
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public decimal DailyTransferLimit { get; set; }
    public decimal MonthlyTransferLimit { get; set; }
    public decimal PerTransactionMax { get; set; }
    public decimal PerTransactionMin { get; set; }
    public DateTime? LastDailyReset { get; set; }
    public DateTime? LastMonthlyReset { get; set; }
    public decimal DailyTransferUsed { get; set; }
    public decimal MonthlyTransferUsed { get; set; }
    
    // Navigation property
    public virtual Account Account { get; set; }
}
```

#### New Entity: ScheduledTransfer
```csharp
public class ScheduledTransfer
{
    public int Id { get; set; }
    public int SourceAccountId { get; set; }
    public int? DestinationAccountId { get; set; }
    public string? DestinationAccountNumber { get; set; }
    public string TransferType { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string RecurrenceType { get; set; } // "OneTime", "Daily", "Weekly", "Monthly", "Quarterly", "Annually"
    public int? RecurrenceDay { get; set; } // Day of month or day of week
    public string Status { get; set; } // "Active", "Paused", "Completed", "Cancelled"
    public DateTime? NextExecutionDate { get; set; }
    public DateTime? LastExecutionDate { get; set; }
    public int ExecutionCount { get; set; }
    public DateTime CreatedDate { get; set; }
    
    // Navigation properties
    public virtual Account SourceAccount { get; set; }
    public virtual Account? DestinationAccount { get; set; }
}
```

### API Endpoints

#### TransfersController
**Endpoints:**
- `POST /api/transfers/internal` - Create internal transfer (between own accounts)
- `POST /api/transfers/external` - Create external transfer (to other account)
- `GET /api/transfers` - Get all transfers (with filtering)
- `GET /api/transfers/{id}` - Get transfer by ID
- `GET /api/accounts/{accountId}/transfers` - Get transfers for specific account
- `PUT /api/transfers/{id}/cancel` - Cancel pending transfer
- `POST /api/transfers/{id}/retry` - Retry failed transfer

#### ScheduledTransfersController
**Endpoints:**
- `POST /api/scheduled-transfers` - Create scheduled/recurring transfer
- `GET /api/scheduled-transfers` - Get all scheduled transfers
- `GET /api/scheduled-transfers/{id}` - Get scheduled transfer by ID
- `PUT /api/scheduled-transfers/{id}` - Update scheduled transfer
- `DELETE /api/scheduled-transfers/{id}` - Cancel scheduled transfer
- `PUT /api/scheduled-transfers/{id}/pause` - Pause recurring transfer
- `PUT /api/scheduled-transfers/{id}/resume` - Resume recurring transfer

#### AccountLimitsController (Optional - Admin)
**Endpoints:**
- `GET /api/accounts/{accountId}/limits` - Get account limits
- `PUT /api/accounts/{accountId}/limits` - Update account limits (admin only)

### DTOs Required

#### TransferDto
```csharp
public class TransferDto
{
    public int Id { get; set; }
    public int SourceAccountId { get; set; }
    public string SourceAccountNumber { get; set; }
    public int? DestinationAccountId { get; set; }
    public string? DestinationAccountNumber { get; set; }
    public string TransferType { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public DateTime TransferDate { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? FailureReason { get; set; }
}
```

#### CreateInternalTransferDto
```csharp
public class CreateInternalTransferDto
{
    [Required]
    public int SourceAccountId { get; set; }
    
    [Required]
    public int DestinationAccountId { get; set; }
    
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; }
    
    public DateTime? ScheduledDate { get; set; }
}
```

#### CreateExternalTransferDto
```csharp
public class CreateExternalTransferDto
{
    [Required]
    public int SourceAccountId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string DestinationAccountNumber { get; set; }
    
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; }
    
    public DateTime? ScheduledDate { get; set; }
}
```

#### CreateScheduledTransferDto
```csharp
public class CreateScheduledTransferDto
{
    [Required]
    public int SourceAccountId { get; set; }
    
    public int? DestinationAccountId { get; set; }
    
    [StringLength(50)]
    public string? DestinationAccountNumber { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; }
    
    [Required]
    public DateTime ScheduledDate { get; set; }
    
    [Required]
    public string RecurrenceType { get; set; } // "OneTime", "Daily", "Weekly", "Monthly", etc.
    
    public int? RecurrenceDay { get; set; } // For weekly/monthly
}
```

### Business Logic Requirements

#### Transfer Processing Service
Create a `TransferService` class to handle:
- Transfer validation (balance, limits, account status)
- Transfer execution (atomic operations with transaction rollback)
- Limit checking and updating
- Transfer status management
- Error handling and retry logic

**Key Methods:**
- `ValidateTransfer(CreateTransferDto dto)` - Validate transfer before execution
- `ExecuteInternalTransfer(CreateInternalTransferDto dto)` - Execute internal transfer
- `ExecuteExternalTransfer(CreateExternalTransferDto dto)` - Execute external transfer
- `CheckTransferLimits(int accountId, decimal amount)` - Check if transfer within limits
- `UpdateTransferLimits(int accountId, decimal amount)` - Update limit usage
- `CancelTransfer(int transferId)` - Cancel pending transfer
- `RetryFailedTransfer(int transferId)` - Retry failed transfer

#### Scheduled Transfer Processor
Create a background service or scheduled job to:
- Process scheduled transfers at their execution time
- Handle recurring transfers
- Update next execution dates
- Handle failures and retries

### Frontend Requirements

#### Transfer Components
- `InternalTransferForm.tsx` - Form for internal transfers
- `ExternalTransferForm.tsx` - Form for external transfers
- `ScheduledTransferForm.tsx` - Form for scheduled/recurring transfers
- `TransferHistory.tsx` - List of past transfers
- `ScheduledTransfersList.tsx` - List of scheduled transfers
- `TransferLimitsDisplay.tsx` - Show current limits and usage

#### Transfer Service
- `transfers.service.ts` - API service for transfer operations
- `scheduledTransfers.service.ts` - API service for scheduled transfers

### Security & Validation

1. **Balance Validation:**
   - Ensure sufficient balance before transfer
   - Handle concurrent transfer attempts (optimistic locking)

2. **Account Validation:**
   - Verify source account exists and is active
   - Verify destination account exists and is active (for external)
   - Prevent transfers to same account

3. **Limit Enforcement:**
   - Check daily limits before transfer
   - Check monthly limits before transfer
   - Check per-transaction limits
   - Reset limits at appropriate intervals

4. **Transaction Integrity:**
   - Use database transactions for atomic operations
   - Rollback on any failure
   - Ensure both accounts updated or neither updated

5. **Audit Trail:**
   - Log all transfer attempts
   - Log limit violations
   - Track transfer history

## Acceptance Criteria

- [ ] Transfer entity and database migration created
- [ ] AccountLimits entity and migration created
- [ ] ScheduledTransfer entity and migration created
- [ ] TransfersController with all endpoints implemented
- [ ] ScheduledTransfersController with all endpoints implemented
- [ ] TransferService with business logic implemented
- [ ] Internal transfer functionality working (between own accounts)
- [ ] External transfer functionality working (to other accounts)
- [ ] Transaction limits enforced (daily, monthly, per-transaction)
- [ ] Limit tracking and reset logic implemented
- [ ] Scheduled transfer creation and management working
- [ ] Recurring transfer functionality working
- [ ] Background job/service for processing scheduled transfers
- [ ] Transfer cancellation functionality
- [ ] Transfer retry functionality for failed transfers
- [ ] Frontend components for all transfer types
- [ ] Transfer history and tracking UI
- [ ] Scheduled transfers management UI
- [ ] Unit tests for TransferService (80%+ coverage)
- [ ] Unit tests for controllers (80%+ coverage)
- [ ] Integration tests for transfer workflows
- [ ] Error handling and validation implemented
- [ ] Swagger documentation updated

## Testing Requirements

### Unit Tests
- Test transfer validation logic
- Test limit checking and enforcement
- Test balance calculations
- Test transfer execution (success and failure scenarios)
- Test scheduled transfer processing
- Test recurrence pattern calculations

### Integration Tests
- Test complete internal transfer flow
- Test complete external transfer flow
- Test limit enforcement
- Test concurrent transfer handling
- Test scheduled transfer execution
- Test transfer cancellation
- Test transfer retry

### Edge Cases
- Insufficient balance scenarios
- Limit exceeded scenarios
- Invalid account number scenarios
- Concurrent transfer attempts
- Scheduled transfer on non-business days
- Recurring transfer with end date

## Priority
**Critical** - This is a core banking functionality that transforms the platform into a real banking system.

## Estimated Effort
Large - 5-7 days

## Dependencies
- Issue #2 (API Controllers) - ✅ Completed
- Issue #3 (Dashboard & Navigation) - ✅ Completed

## Related Files
- `MyApp.Core/Entities/Transfer.cs` (to be created)
- `MyApp.Core/Entities/AccountLimits.cs` (to be created)
- `MyApp.Core/Entities/ScheduledTransfer.cs` (to be created)
- `MyApp.Core/DTOs/TransferDto.cs` (to be created)
- `MyApp/Controllers/TransfersController.cs` (to be created)
- `MyApp/Controllers/ScheduledTransfersController.cs` (to be created)
- `MyApp/Services/TransferService.cs` (to be created)
- `MyApp/Services/ScheduledTransferProcessor.cs` (to be created)
- `BankUI/src/components/InternalTransferForm.tsx` (to be created)
- `BankUI/src/components/ExternalTransferForm.tsx` (to be created)
- `BankUI/src/components/ScheduledTransferForm.tsx` (to be created)

## Future Enhancements (Out of Scope for Issue #4)
- Wire transfers
- ACH transfers
- International transfers
- Transfer fees
- Transfer approval workflows (for large amounts)
- Multi-currency transfers
- Transfer notifications/email confirmations
- Transfer templates/favorites


