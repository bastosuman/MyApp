import { apiService } from './api.service';
import { ApiResponse, Transaction, CreateTransaction } from '../types';

export const transactionsService = {
  /**
   * Get all transactions with pagination
   */
  async getAll(page: number = 1, pageSize: number = 50): Promise<ApiResponse<Transaction[]>> {
    return apiService.get<ApiResponse<Transaction[]>>('/api/transactions', {
      params: { page, pageSize },
    });
  },

  /**
   * Get transaction by ID
   */
  async getById(id: number): Promise<ApiResponse<Transaction>> {
    return apiService.get<ApiResponse<Transaction>>(`/api/transactions/${id}`);
  },

  /**
   * Get transactions by account ID
   */
  async getByAccount(accountId: number): Promise<ApiResponse<Transaction[]>> {
    return apiService.get<ApiResponse<Transaction[]>>(`/api/accounts/${accountId}/transactions`);
  },

  /**
   * Create a new transaction
   */
  async create(transaction: CreateTransaction): Promise<ApiResponse<Transaction>> {
    return apiService.post<ApiResponse<Transaction>>('/api/transactions', transaction);
  },
};

