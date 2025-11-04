import { apiService } from './api.service';
import { ApiResponse, Account, CreateAccount, UpdateAccount, Transaction } from '../types';

export const accountsService = {
  /**
   * Get all accounts
   */
  async getAll(): Promise<ApiResponse<Account[]>> {
    return apiService.get<ApiResponse<Account[]>>('/api/accounts');
  },

  /**
   * Get account by ID
   */
  async getById(id: number): Promise<ApiResponse<Account>> {
    return apiService.get<ApiResponse<Account>>(`/api/accounts/${id}`);
  },

  /**
   * Create a new account
   */
  async create(account: CreateAccount): Promise<ApiResponse<Account>> {
    return apiService.post<ApiResponse<Account>>('/api/accounts', account);
  },

  /**
   * Update an existing account
   */
  async update(id: number, account: UpdateAccount): Promise<ApiResponse<Account>> {
    return apiService.put<ApiResponse<Account>>(`/api/accounts/${id}`, account);
  },

  /**
   * Get transactions for a specific account
   */
  async getTransactions(accountId: number): Promise<ApiResponse<Transaction[]>> {
    return apiService.get<ApiResponse<Transaction[]>>(`/api/accounts/${accountId}/transactions`);
  },

  /**
   * Get applications for a specific account
   */
  async getApplications(accountId: number): Promise<ApiResponse<any[]>> {
    return apiService.get<ApiResponse<any[]>>(`/api/accounts/${accountId}/applications`);
  },
};

