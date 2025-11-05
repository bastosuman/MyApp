import { apiService } from './api.service';
import { ApiResponse } from '../types';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  success: boolean;
  message?: string;
  user?: {
    id: number;
    username: string;
    role: string;
  };
  token?: string;
}

export const authService = {
  async login(credentials: LoginRequest): Promise<ApiResponse<LoginResponse>> {
    return apiService.post<ApiResponse<LoginResponse>>('/api/auth/login', credentials);
  },

  async logout(): Promise<void> {
    localStorage.removeItem('user');
    localStorage.removeItem('authToken');
  },

  getCurrentUser(): LoginResponse['user'] | null {
    const userStr = localStorage.getItem('user');
    if (!userStr) return null;
    try {
      return JSON.parse(userStr);
    } catch {
      return null;
    }
  },

  setUser(user: LoginResponse['user']): void {
    if (user) {
      localStorage.setItem('user', JSON.stringify(user));
    } else {
      localStorage.removeItem('user');
    }
  },

  isAuthenticated(): boolean {
    return this.getCurrentUser() !== null;
  }
};


