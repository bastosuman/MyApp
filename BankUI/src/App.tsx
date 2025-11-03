import { useState, useEffect } from 'react';
import './App.css';
import { WeatherForecast } from './types/weather';
import { weatherService } from './services/weather.service';
import { API_CONFIG } from './config/api.config';

function App() {
  const [weatherData, setWeatherData] = useState<WeatherForecast[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [apiUrl, setApiUrl] = useState<string>('');

  const fetchWeatherData = async () => {
    setLoading(true);
    setError(null);
    try {
      console.log('Fetching weather data...');
      const data = await weatherService.getWeatherForecast();
      console.log('Data received:', data);
      console.log('Data type:', typeof data);
      console.log('Is array?', Array.isArray(data));
      console.log('Data length:', Array.isArray(data) ? data.length : 'Not an array');
      
      if (Array.isArray(data)) {
        console.log('First item:', data[0]);
        console.log('First item keys:', Object.keys(data[0] || {}));
        // Ensure data is properly formatted
        const formattedData = data.map((item: any) => ({
          date: item.date || item.Date || 'N/A',
          temperatureC: item.temperatureC ?? item.TemperatureC ?? 0,
          temperatureF: item.temperatureF ?? item.TemperatureF ?? 0,
          summary: item.summary || item.Summary || null,
        }));
        console.log('Formatted data:', formattedData);
        setWeatherData(formattedData);
      } else {
        console.warn('Data is not an array:', data);
        setWeatherData([]);
      }
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || err.message || 'Failed to fetch data from MyApp backend';
      setError(errorMessage);
      console.error('Error fetching weather data:', err);
      if (err.response) {
        console.error('Response status:', err.response.status);
        console.error('Response data:', err.response.data);
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    // Display the API URL being used
    setApiUrl(`${API_CONFIG.baseURL}${API_CONFIG.endpoints.weatherForecast}`);
  }, []);

  return (
    <div className="App">
      <header className="App-header">
        <h1>BankUI - Connected to MyApp Backend</h1>
        <p>This frontend is connected to the MyApp .NET backend API</p>
        {apiUrl && (
          <p style={{ fontSize: '0.9em', color: '#666', marginBottom: '20px' }}>
            API Endpoint: <code>{apiUrl}</code>
          </p>
        )}
        
        <div className="api-section">
          <button onClick={fetchWeatherData} disabled={loading}>
            {loading ? 'Loading...' : 'Fetch Data from MyApp API'}
          </button>

          {error && (
            <div className="error-message">
              <p>Error: {error}</p>
              <p className="error-hint">
                Make sure MyApp backend is running on http://localhost:5250
                <br />
                Check the browser console (F12) for more details
              </p>
            </div>
          )}

          {weatherData.length > 0 ? (
            <div className="weather-data">
              <h2>Weather Forecast Data from MyApp:</h2>
              <p style={{ fontSize: '0.9em', color: '#28a745', marginBottom: '10px' }}>
                ✓ Successfully loaded {weatherData.length} record{weatherData.length !== 1 ? 's' : ''}
              </p>
              {weatherData.length > 0 && (
                <div style={{ fontSize: '0.8em', color: '#666', marginBottom: '10px', fontStyle: 'italic' }}>
                  Debug: First item preview - {JSON.stringify(weatherData[0]).substring(0, 100)}...
                </div>
              )}
              <table>
                <thead>
                  <tr>
                    <th>Date</th>
                    <th>Temperature (°C)</th>
                    <th>Temperature (°F)</th>
                    <th>Summary</th>
                  </tr>
                </thead>
                <tbody>
                  {weatherData.length === 0 ? (
                    <tr>
                      <td colSpan={4} style={{ textAlign: 'center', padding: '20px', color: '#999' }}>
                        No data available
                      </td>
                    </tr>
                  ) : (
                    weatherData.map((forecast, index) => {
                      console.log('Rendering forecast:', index, forecast);
                      return (
                        <tr key={`forecast-${index}`}>
                          <td style={{ color: '#000000' }}>{String(forecast?.date || 'N/A')}</td>
                          <td style={{ color: '#000000' }}>{String(forecast?.temperatureC ?? 'N/A')}</td>
                          <td style={{ color: '#000000' }}>{String(forecast?.temperatureF ?? 'N/A')}</td>
                          <td style={{ color: '#000000' }}>{String(forecast?.summary || 'N/A')}</td>
                        </tr>
                      );
                    })
                  )}
                </tbody>
              </table>
            </div>
          ) : (
            weatherData.length === 0 && !loading && !error && (
              <div style={{ marginTop: '20px', padding: '15px', background: '#f8f9fa', borderRadius: '6px', color: '#666' }}>
                <p>No data loaded. Click the button above to fetch weather forecast data.</p>
              </div>
            )
          )}
        </div>
      </header>
    </div>
  );
}

export default App;

