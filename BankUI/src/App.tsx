import { useState } from 'react';
import './App.css';
import { Accounts } from './components/Accounts';
import { Products } from './components/Products';
import { Transactions } from './components/Transactions';
import { Applications } from './components/Applications';

type Tab = 'accounts' | 'products' | 'transactions' | 'applications';

function App() {
  const [activeTab, setActiveTab] = useState<Tab>('accounts');

  return (
    <div className="App">
      <header className="App-header">
        <h1>MyApp Financial Services</h1>
        <nav className="nav-tabs">
          <button
            className={activeTab === 'accounts' ? 'active' : ''}
            onClick={() => setActiveTab('accounts')}
          >
            Accounts
          </button>
          <button
            className={activeTab === 'products' ? 'active' : ''}
            onClick={() => setActiveTab('products')}
          >
            Products
          </button>
          <button
            className={activeTab === 'transactions' ? 'active' : ''}
            onClick={() => setActiveTab('transactions')}
          >
            Transactions
          </button>
          <button
            className={activeTab === 'applications' ? 'active' : ''}
            onClick={() => setActiveTab('applications')}
          >
            Applications
          </button>
        </nav>
      </header>

      <main className="App-main">
        {activeTab === 'accounts' && <Accounts />}
        {activeTab === 'products' && <Products />}
        {activeTab === 'transactions' && <Transactions />}
        {activeTab === 'applications' && <Applications />}
      </main>
    </div>
  );
}

export default App;
