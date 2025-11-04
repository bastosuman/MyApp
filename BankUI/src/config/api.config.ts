/**
 * API Configuration for connecting to MyApp backend
 */
export const API_CONFIG = {
  // Backend API base URL
  // In development, you can use HTTP (http://localhost:5250) to avoid SSL certificate issues
  // In production, use HTTPS
  baseURL: import.meta.env.VITE_API_BASE_URL || 
    (import.meta.env.DEV ? 'http://localhost:5250' : 'https://localhost:7059'),
  
  // API endpoints
  endpoints: {
    // Financial endpoints will be added here
  },
  
  // Request timeout in milliseconds
  timeout: 10000,
  
  // Default headers
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
  },
} as const;

/**
 * Get full API URL for an endpoint
 */
export const getApiUrl = (endpoint: string): string => {
  return `${API_CONFIG.baseURL}${endpoint}`;
};

