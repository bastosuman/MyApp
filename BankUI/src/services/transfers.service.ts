import { apiService } from './api.service';
import { ApiResponse, Transfer, CreateInternalTransfer, CreateExternalTransfer, ScheduledTransfer, CreateScheduledTransfer, UpdateScheduledTransfer } from '../types';

export const transfersService = {
  /**
   * Create an internal transfer (between own accounts)
   */
  async createInternal(transfer: CreateInternalTransfer): Promise<ApiResponse<Transfer>> {
    return apiService.post<ApiResponse<Transfer>>('/api/transfers/internal', transfer);
  },

  /**
   * Create an external transfer (to another account by account number)
   */
  async createExternal(transfer: CreateExternalTransfer): Promise<ApiResponse<Transfer>> {
    return apiService.post<ApiResponse<Transfer>>('/api/transfers/external', transfer);
  },

  /**
   * Get all transfers with optional filters
   */
  async getAll(accountId?: number, status?: string, transferType?: string): Promise<ApiResponse<Transfer[]>> {
    const params = new URLSearchParams();
    if (accountId) params.append('accountId', accountId.toString());
    if (status) params.append('status', status);
    if (transferType) params.append('transferType', transferType);
    
    const query = params.toString();
    return apiService.get<ApiResponse<Transfer[]>>(`/api/transfers${query ? `?${query}` : ''}`);
  },

  /**
   * Get transfer by ID
   */
  async getById(id: number): Promise<ApiResponse<Transfer>> {
    return apiService.get<ApiResponse<Transfer>>(`/api/transfers/${id}`);
  },

  /**
   * Get transfers for a specific account
   */
  async getByAccount(accountId: number): Promise<ApiResponse<Transfer[]>> {
    return apiService.get<ApiResponse<Transfer[]>>(`/api/transfers/accounts/${accountId}`);
  },

  /**
   * Cancel a pending transfer
   */
  async cancel(id: number): Promise<ApiResponse<any>> {
    return apiService.put<ApiResponse<any>>(`/api/transfers/${id}/cancel`, {});
  },

  /**
   * Retry a failed transfer
   */
  async retry(id: number): Promise<ApiResponse<Transfer>> {
    return apiService.post<ApiResponse<Transfer>>(`/api/transfers/${id}/retry`, {});
  },

  /**
   * Scheduled Transfers
   */
  async createScheduled(transfer: CreateScheduledTransfer): Promise<ApiResponse<ScheduledTransfer>> {
    return apiService.post<ApiResponse<ScheduledTransfer>>('/api/scheduledtransfers', transfer);
  },

  async getScheduled(accountId?: number, status?: string): Promise<ApiResponse<ScheduledTransfer[]>> {
    const params = new URLSearchParams();
    if (accountId) params.append('accountId', accountId.toString());
    if (status) params.append('status', status);
    
    const query = params.toString();
    return apiService.get<ApiResponse<ScheduledTransfer[]>>(`/api/scheduledtransfers${query ? `?${query}` : ''}`);
  },

  async getScheduledById(id: number): Promise<ApiResponse<ScheduledTransfer>> {
    return apiService.get<ApiResponse<ScheduledTransfer>>(`/api/scheduledtransfers/${id}`);
  },

  async updateScheduled(id: number, transfer: UpdateScheduledTransfer): Promise<ApiResponse<ScheduledTransfer>> {
    return apiService.put<ApiResponse<ScheduledTransfer>>(`/api/scheduledtransfers/${id}`, transfer);
  },

  async cancelScheduled(id: number): Promise<ApiResponse<any>> {
    return apiService.delete<ApiResponse<any>>(`/api/scheduledtransfers/${id}`);
  },

  async pauseScheduled(id: number): Promise<ApiResponse<any>> {
    return apiService.put<ApiResponse<any>>(`/api/scheduledtransfers/${id}/pause`, {});
  },

  async resumeScheduled(id: number): Promise<ApiResponse<any>> {
    return apiService.put<ApiResponse<any>>(`/api/scheduledtransfers/${id}/resume`, {});
  },
};


