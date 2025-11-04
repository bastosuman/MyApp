import { apiService } from './api.service';
import { ApiResponse, Product } from '../types';

export const productsService = {
  /**
   * Get all products
   */
  async getAll(includeInactive: boolean = false): Promise<ApiResponse<Product[]>> {
    return apiService.get<ApiResponse<Product[]>>('/api/products', {
      params: { includeInactive },
    });
  },

  /**
   * Get product by ID
   */
  async getById(id: number): Promise<ApiResponse<Product>> {
    return apiService.get<ApiResponse<Product>>(`/api/products/${id}`);
  },
};

