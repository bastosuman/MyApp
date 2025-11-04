import { apiService } from './api.service';
import { ApiResponse, Application, CreateApplication, UpdateApplicationStatus } from '../types';

export const applicationsService = {
  /**
   * Get all applications with optional status filter
   */
  async getAll(status?: string): Promise<ApiResponse<Application[]>> {
    const params = status ? { status } : {};
    return apiService.get<ApiResponse<Application[]>>('/api/applications', {
      params,
    });
  },

  /**
   * Get application by ID
   */
  async getById(id: number): Promise<ApiResponse<Application>> {
    return apiService.get<ApiResponse<Application>>(`/api/applications/${id}`);
  },

  /**
   * Create a new application
   */
  async create(application: CreateApplication): Promise<ApiResponse<Application>> {
    return apiService.post<ApiResponse<Application>>('/api/applications', application);
  },

  /**
   * Update application status
   */
  async updateStatus(id: number, update: UpdateApplicationStatus): Promise<ApiResponse<Application>> {
    return apiService.put<ApiResponse<Application>>(`/api/applications/${id}/status`, update);
  },
};

