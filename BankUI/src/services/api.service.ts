import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios';
import { API_CONFIG, getApiUrl } from '../config/api.config';

/**
 * API Service for communicating with MyApp backend
 */
class ApiService {
  private client: AxiosInstance;

  constructor() {
    this.client = axios.create({
      baseURL: API_CONFIG.baseURL,
      timeout: API_CONFIG.timeout,
      headers: API_CONFIG.headers,
    });

    // Request interceptor
    this.client.interceptors.request.use(
      (config) => {
        // Add auth token or other headers here if needed
        // const token = localStorage.getItem('authToken');
        // if (token) {
        //   config.headers.Authorization = `Bearer ${token}`;
        // }
        return config;
      },
      (error) => {
        return Promise.reject(error);
      }
    );

    // Response interceptor
    this.client.interceptors.response.use(
      (response) => {
        return response;
      },
      (error) => {
        // Handle errors globally
        if (error.response) {
          // Server responded with error status
          console.error('API Error:', error.response.data);
        } else if (error.request) {
          // Request made but no response received
          console.error('Network Error:', error.message);
        } else {
          // Something else happened
          console.error('Error:', error.message);
        }
        return Promise.reject(error);
      }
    );
  }

  /**
   * Generic GET request
   */
  async get<T = any>(endpoint: string, config?: AxiosRequestConfig): Promise<T> {
    const response: AxiosResponse<T> = await this.client.get(endpoint, config);
    return response.data;
  }

  /**
   * Generic POST request
   */
  async post<T = any>(endpoint: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response: AxiosResponse<T> = await this.client.post(endpoint, data, config);
    return response.data;
  }

  /**
   * Generic PUT request
   */
  async put<T = any>(endpoint: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response: AxiosResponse<T> = await this.client.put(endpoint, data, config);
    return response.data;
  }

  /**
   * Generic DELETE request
   */
  async delete<T = any>(endpoint: string, config?: AxiosRequestConfig): Promise<T> {
    const response: AxiosResponse<T> = await this.client.delete(endpoint, config);
    return response.data;
  }
}

// Export singleton instance
export const apiService = new ApiService();

