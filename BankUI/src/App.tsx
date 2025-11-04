import { useState } from 'react';
import './App.css';
import { API_CONFIG } from './config/api.config';

function App() {
  const [apiUrl] = useState<string>(API_CONFIG.baseURL);

  return (
    <div className="App">
      <header className="App-header">
        <h1>MyApp Financial Services</h1>
        <p>Financial services platform connected to the MyApp .NET backend</p>
        {apiUrl && (
          <p style={{ fontSize: '0.9em', color: '#666', marginBottom: '20px' }}>
            API Endpoint: <code>{apiUrl}</code>
          </p>
        )}
        
        <div className="api-section">
          <div style={{ marginTop: '20px', padding: '15px', background: '#f8f9fa', borderRadius: '6px', color: '#666' }}>
            <p>Welcome to the financial services application.</p>
            <p style={{ marginTop: '10px', fontSize: '0.9em' }}>
              Financial features will be implemented here.
            </p>
          </div>
        </div>
      </header>
    </div>
  );
}

export default App;
