import { apiService } from './api.service';
import { API_CONFIG } from '../config/api.config';
import { WeatherForecast } from '../types/weather';

/**
 * Weather Service - Example service connecting to MyApp backend
 */
export const weatherService = {
  /**
   * Get weather forecast from MyApp backend
   */
  async getWeatherForecast(): Promise<WeatherForecast[]> {
    try {
      console.log('Fetching weather forecast from:', `${API_CONFIG.baseURL}${API_CONFIG.endpoints.weatherForecast}`);
      const data = await apiService.get<WeatherForecast[]>(
        API_CONFIG.endpoints.weatherForecast
      );
      console.log('Received weather data:', data);
      return data;
    } catch (error) {
      console.error('Failed to fetch weather forecast:', error);
      throw error;
    }
  },
};

