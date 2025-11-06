/**
 * TypeScript types matching the backend DTOs
 */

// API Response wrapper
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message: string;
  errors?: string[];
}

// Account types
export interface Account {
  id: number;
  accountNumber: string;
  accountHolderName: string;
  balance: number;
  accountType: string;
  createdDate: string;
  isActive: boolean;
}

export interface CreateAccount {
  accountNumber: string;
  accountHolderName: string;
  balance: number;
  accountType: string;
}

export interface UpdateAccount {
  accountHolderName: string;
  balance: number;
  accountType: string;
  isActive: boolean;
}

// Product types
export interface Product {
  id: number;
  name: string;
  productType: string;
  interestRate: number;
  minAmount: number;
  maxAmount: number;
  description: string;
  isActive: boolean;
  createdDate: string;
}

// Transaction types
export interface Transaction {
  id: number;
  accountId: number;
  transactionType: string;
  amount: number;
  description: string;
  transactionDate: string;
  status: string;
  accountNumber?: string;
}

export interface CreateTransaction {
  accountId: number;
  transactionType: string;
  amount: number;
  description: string;
  transactionDate?: string;
}

// Application types
export interface Application {
  id: number;
  accountId: number;
  productId: number;
  requestedAmount: number;
  status: string;
  applicationDate: string;
  decisionDate?: string;
  notes?: string;
  accountNumber?: string;
  productName?: string;
}

export interface CreateApplication {
  accountId: number;
  productId: number;
  requestedAmount: number;
  notes?: string;
}

export interface UpdateApplicationStatus {
  status: string;
  notes?: string;
}

// Transfer types
export interface Transfer {
  id: number;
  sourceAccountId: number;
  sourceAccountNumber: string;
  destinationAccountId?: number;
  destinationAccountNumber?: string;
  transferType: string;
  amount: number;
  description: string;
  status: string;
  transferDate: string;
  scheduledDate?: string;
  completedDate?: string;
  failureReason?: string;
}

export interface CreateInternalTransfer {
  sourceAccountId: number;
  destinationAccountId: number;
  amount: number;
  description: string;
  scheduledDate?: string;
}

export interface CreateExternalTransfer {
  sourceAccountId: number;
  destinationAccountNumber: string;
  amount: number;
  description: string;
  scheduledDate?: string;
}

// Scheduled Transfer types
export interface ScheduledTransfer {
  id: number;
  sourceAccountId: number;
  sourceAccountNumber: string;
  destinationAccountId?: number;
  destinationAccountNumber?: string;
  transferType: string;
  amount: number;
  description: string;
  scheduledDate: string;
  recurrenceType: string;
  recurrenceDay?: number;
  status: string;
  nextExecutionDate?: string;
  lastExecutionDate?: string;
  executionCount: number;
  createdDate: string;
}

export interface CreateScheduledTransfer {
  sourceAccountId: number;
  destinationAccountId?: number;
  destinationAccountNumber?: string;
  amount: number;
  description: string;
  scheduledDate: string;
  recurrenceType: string;
  recurrenceDay?: number;
}

export interface UpdateScheduledTransfer {
  amount?: number;
  description?: string;
  scheduledDate?: string;
  recurrenceType?: string;
  recurrenceDay?: number;
}

