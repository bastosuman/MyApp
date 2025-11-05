import { apiService } from './api.service';
import { ApiResponse, Account, Transaction } from '../types';

export interface DashboardData {
  accountSummary: {
    totalBalance: number;
    accountCount: number;
    accounts: Account[];
  };
  recentTransactions: Transaction[];
  applicationStatus: {
    pending: number;
    approved: number;
    rejected: number;
    total: number;
  };
  availableProductsCount: number;
}

export const dashboardService = {
  /**
   * Get dashboard summary data
   */
  async getDashboard(): Promise<ApiResponse<DashboardData>> {
    return apiService.get<ApiResponse<DashboardData>>('/api/dashboard');
  },
};

