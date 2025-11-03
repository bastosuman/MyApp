# Issue 5: Frontend Integration - Connect BankUI to Backend APIs

**Labels:** `enhancement`, `frontend`, `api`, `integration`

---

## 🎯 Goal
Integrate the BankUI React frontend with all backend APIs (Accounts, Transactions, Applications, Products, Calculations) and create user-friendly interfaces for financial operations.

## ✅ Prerequisites
- [x] All backend APIs complete (Issues #1-4)
- [x] BankUI React application exists
- [x] CORS configured in backend
- [x] Backend running on localhost:5250

## 📋 Tasks

### Step 1: Update API Configuration
- [ ] Update `BankUI/src/config/api.config.ts`
- [ ] Ensure base URL points to `http://localhost:5250`
- [ ] Verify CORS is working
- [ ] Test connection to backend

### Step 2: Create API Service Layer
- [ ] Create `BankUI/src/services/accounts.service.ts`
  - [ ] `getAllAccounts()` - Fetch all accounts
  - [ ] `getAccountById(id)` - Get account details
  - [ ] `createAccount(data)` - Create new account
  - [ ] `updateAccount(id, data)` - Update account
  - [ ] `deleteAccount(id)` - Delete account
  
- [ ] Create `BankUI/src/services/transactions.service.ts`
  - [ ] `getAllTransactions(filters?)` - Fetch transactions
  - [ ] `getTransactionById(id)` - Get transaction details
  - [ ] `getAccountTransactions(accountId)` - Get account transactions
  - [ ] `createTransaction(data)` - Create transaction
  
- [ ] Create `BankUI/src/services/applications.service.ts`
  - [ ] `getAllApplications(filters?)` - Fetch applications
  - [ ] `getApplicationById(id)` - Get application details
  - [ ] `submitApplication(data)` - Submit new application
  - [ ] `approveApplication(id)` - Approve application
  - [ ] `rejectApplication(id, reason)` - Reject application
  
- [ ] Create `BankUI/src/services/products.service.ts`
  - [ ] `getAllProducts(filters?)` - Fetch products
  - [ ] `getProductById(id)` - Get product details
  
- [ ] Update `BankUI/src/services/calculations.service.ts`
  - [ ] `calculateInterest(data)` - Calculate interest
  - [ ] `calculateMonthlyPayment(data)` - Calculate payment
  - [ ] `calculateCreditScore(data)` - Calculate credit score

### Step 3: Create TypeScript Types
- [ ] Create `BankUI/src/types/account.ts`
  - [ ] Account interface
  - [ ] CreateAccountRequest interface
  - [ ] UpdateAccountRequest interface
  
- [ ] Create `BankUI/src/types/transaction.ts`
  - [ ] Transaction interface
  - [ ] CreateTransactionRequest interface
  
- [ ] Create `BankUI/src/types/application.ts`
  - [ ] Application interface
  - [ ] SubmitApplicationRequest interface
  
- [ ] Create `BankUI/src/types/product.ts`
  - [ ] Product interface
  
- [ ] Update `BankUI/src/types/calculations.ts`
  - [ ] Calculation request/response types

### Step 4: Build Account Management UI
- [ ] Create `BankUI/src/components/accounts/AccountList.tsx`
  - [ ] Display list of accounts with pagination
  - [ ] Filter by status, search by email/name
  - [ ] Link to account details
  
- [ ] Create `BankUI/src/components/accounts/AccountDetails.tsx`
  - [ ] Display account information
  - [ ] Show related transactions
  - [ ] Show related applications
  - [ ] Edit account button
  
- [ ] Create `BankUI/src/components/accounts/AccountForm.tsx`
  - [ ] Create new account form
  - [ ] Edit account form
  - [ ] Form validation
  - [ ] Error handling
  
- [ ] Add routing in `App.tsx` for account pages

### Step 5: Build Transaction Management UI
- [ ] Create `BankUI/src/components/transactions/TransactionList.tsx`
  - [ ] Display transactions with filters
  - [ ] Filter by account, type, status, date range
  - [ ] Pagination support
  
- [ ] Create `BankUI/src/components/transactions/TransactionForm.tsx`
  - [ ] Create new transaction form
  - [ ] Account selection dropdown
  - [ ] Transaction type selection
  - [ ] Amount and description fields
  
- [ ] Create `BankUI/src/components/transactions/TransactionDetails.tsx`
  - [ ] Display transaction details
  - [ ] Update transaction status (if pending)

### Step 6: Build Application Management UI
- [ ] Create `BankUI/src/components/applications/ApplicationList.tsx`
  - [ ] Display applications with filters
  - [ ] Show application status (Pending/Approved/Rejected)
  - [ ] Link to application details
  
- [ ] Create `BankUI/src/components/applications/ApplicationForm.tsx`
  - [ ] Product selection dropdown
  - [ ] Account selection
  - [ ] Amount input with product limits validation
  - [ ] Term selection with product range validation
  - [ ] Real-time calculation preview (monthly payment, total interest)
  - [ ] Submit application
  
- [ ] Create `BankUI/src/components/applications/ApplicationDetails.tsx`
  - [ ] Display full application details
  - [ ] Show calculated values
  - [ ] Approval/rejection buttons (admin)
  - [ ] Application status display

### Step 7: Build Product Catalog UI
- [ ] Create `BankUI/src/components/products/ProductList.tsx`
  - [ ] Display available products
  - [ ] Filter by product type
  - [ ] Show product details (rates, limits, terms)
  - [ ] "Apply Now" button linking to application form
  
- [ ] Create `BankUI/src/components/products/ProductCard.tsx`
  - [ ] Display product in card format
  - [ ] Show key information (rate, limits)
  - [ ] Link to application form with pre-filled product

### Step 8: Build Calculator Component
- [ ] Create `BankUI/src/components/calculator/LoanCalculator.tsx`
  - [ ] Principal amount input
  - [ ] Interest rate input
  - [ ] Term (months) input
  - [ ] Call calculation API
  - [ ] Display results:
    - Monthly payment
    - Total payment
    - Total interest
  - [ ] Real-time calculation on input change
  
- [ ] Create `BankUI/src/components/calculator/CreditScoreCalculator.tsx`
  - [ ] Income input
  - [ ] Debt input
  - [ ] Credit history input
  - [ ] Bankruptcy checkbox
  - [ ] Display calculated credit score

### Step 9: Build Dashboard
- [ ] Create `BankUI/src/components/dashboard/Dashboard.tsx`
  - [ ] Summary cards (total accounts, transactions, applications)
  - [ ] Recent transactions list
  - [ ] Recent applications list
  - [ ] Quick actions (create account, new transaction)

### Step 10: Error Handling & UX
- [ ] Implement loading states for all API calls
- [ ] Add error messages display
- [ ] Add success notifications
- [ ] Handle network errors gracefully
- [ ] Add form validation feedback
- [ ] Add confirmation dialogs for delete actions

### Step 11: Testing & Polish
- [ ] Test all API integrations
- [ ] Test form validations
- [ ] Test error scenarios
- [ ] Add loading skeletons
- [ ] Improve UI/UX with proper styling
- [ ] Ensure responsive design

## 📝 Component Structure
```
BankUI/src/
├── components/
│   ├── accounts/
│   │   ├── AccountList.tsx
│   │   ├── AccountDetails.tsx
│   │   └── AccountForm.tsx
│   ├── transactions/
│   │   ├── TransactionList.tsx
│   │   ├── TransactionForm.tsx
│   │   └── TransactionDetails.tsx
│   ├── applications/
│   │   ├── ApplicationList.tsx
│   │   ├── ApplicationForm.tsx
│   │   └── ApplicationDetails.tsx
│   ├── products/
│   │   ├── ProductList.tsx
│   │   └── ProductCard.tsx
│   ├── calculator/
│   │   ├── LoanCalculator.tsx
│   │   └── CreditScoreCalculator.tsx
│   └── dashboard/
│       └── Dashboard.tsx
├── services/
│   ├── accounts.service.ts
│   ├── transactions.service.ts
│   ├── applications.service.ts
│   ├── products.service.ts
│   └── calculations.service.ts
└── types/
    ├── account.ts
    ├── transaction.ts
    ├── application.ts
    ├── product.ts
    └── calculations.ts
```

## ✅ Acceptance Criteria
- [ ] All API services created and working
- [ ] All UI components created
- [ ] Forms have proper validation
- [ ] Error handling implemented
- [ ] Loading states shown
- [ ] Success/error notifications displayed
- [ ] All CRUD operations working from UI
- [ ] Calculator components functional
- [ ] Dashboard displays summary data
- [ ] Application tested end-to-end

## 🔗 Related Files
- `BankUI/src/config/api.config.ts`
- `BankUI/src/services/*.service.ts` (to be created/updated)
- `BankUI/src/components/**/*.tsx` (to be created)
- `BankUI/src/types/*.ts` (to be created)


