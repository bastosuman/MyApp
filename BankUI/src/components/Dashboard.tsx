import { useState, useEffect, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { dashboardService, DashboardData } from '../services/dashboard.service';
import { DashboardSkeleton } from './DashboardSkeleton';
import { Icons } from './Icons';
import './Dashboard.css';

export function Dashboard() {
  const [dashboardData, setDashboardData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [refreshing, setRefreshing] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [transactionFilter, setTransactionFilter] = useState<string>('all');
  const navigate = useNavigate();

  useEffect(() => {
    loadDashboard();
  }, []);

  const loadDashboard = async (isRefresh = false) => {
    try {
      if (isRefresh) {
        setRefreshing(true);
      } else {
        setLoading(true);
      }
      setError(null);
      const response = await dashboardService.getDashboard();
      if (response.success && response.data) {
        setDashboardData(response.data);
      } else {
        setError(response.message || 'Failed to load dashboard data');
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load dashboard data');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };


  // Filter transactions based on search and filter
  const filteredTransactions = useMemo(() => {
    if (!dashboardData) return [];
    
    let filtered = dashboardData.recentTransactions;

    // Filter by transaction type
    if (transactionFilter !== 'all') {
      filtered = filtered.filter(t => 
        t.transactionType.toLowerCase() === transactionFilter.toLowerCase()
      );
    }

    // Filter by search term
    if (searchTerm) {
      const searchLower = searchTerm.toLowerCase();
      filtered = filtered.filter(t =>
        t.description.toLowerCase().includes(searchLower) ||
        t.accountNumber?.toLowerCase().includes(searchLower) ||
        t.transactionType.toLowerCase().includes(searchLower) ||
        formatCurrency(t.amount).toLowerCase().includes(searchLower)
      );
    }

    return filtered;
  }, [dashboardData, searchTerm, transactionFilter]);

  // Get transaction type color
  const getTransactionTypeColor = (type: string) => {
    const typeLower = type.toLowerCase();
    if (typeLower.includes('deposit')) return 'positive';
    if (typeLower.includes('withdrawal') || typeLower.includes('debit')) return 'negative';
    if (typeLower.includes('transfer')) return 'neutral';
    return 'default';
  };

  // Get unique transaction types for filter
  const transactionTypes = useMemo(() => {
    if (!dashboardData) return [];
    const types = new Set(dashboardData.recentTransactions.map(t => t.transactionType));
    return Array.from(types);
  }, [dashboardData]);

  if (loading) {
    return <DashboardSkeleton />;
  }

  if (error) {
    return (
      <div className="dashboard">
        <div className="error-state">
          <Icons.XCircle />
          <h3>Error Loading Dashboard</h3>
          <p>{error}</p>
          <button onClick={() => loadDashboard()} className="retry-btn">
            <Icons.Refresh />
            Retry
          </button>
        </div>
      </div>
    );
  }

  if (!dashboardData) {
    return (
      <div className="dashboard">
        <div className="error-state">
          <Icons.XCircle />
          <h3>No Data Available</h3>
          <p>Unable to load dashboard data. Please try again.</p>
          <button onClick={() => loadDashboard()} className="retry-btn">
            <Icons.Refresh />
            Retry
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="dashboard">
      <div className="dashboard-header">
        <div>
          <h2>Dashboard Overview</h2>
          <p className="dashboard-subtitle">Welcome back! Here's your financial summary</p>
        </div>
        <button 
          className={`refresh-btn ${refreshing ? 'refreshing' : ''}`}
          onClick={() => loadDashboard(true)}
          disabled={refreshing}
          title="Refresh data"
        >
          <span className={refreshing ? 'spinning' : ''}>
            <Icons.Refresh />
          </span>
          {refreshing ? 'Refreshing...' : 'Refresh'}
        </button>
      </div>

      <div className="dashboard-grid">
        {/* Account Summary Card */}
        <div className="dashboard-card account-summary">
          <div className="card-icon">
            <Icons.Wallet />
          </div>
          <h3>Total Balance</h3>
          <div className="summary-value">
            {formatCurrency(dashboardData.accountSummary.totalBalance)}
          </div>
          <div className="summary-label">
            Across {dashboardData.accountSummary.accountCount} account{dashboardData.accountSummary.accountCount !== 1 ? 's' : ''}
          </div>
          <button 
            className="card-action-btn"
            onClick={() => navigate('/accounts')}
          >
            View Accounts
            <Icons.ArrowRight />
          </button>
        </div>

        {/* Application Status Card */}
        <div className="dashboard-card application-status">
          <div className="card-icon">
            <Icons.FileText />
          </div>
          <h3>Applications</h3>
          <div className="status-grid">
            <div className="status-item">
              <div className="status-value pending">
                {dashboardData.applicationStatus.pending}
              </div>
              <div className="status-label">Pending</div>
            </div>
            <div className="status-item">
              <div className="status-value approved">
                {dashboardData.applicationStatus.approved}
              </div>
              <div className="status-label">Approved</div>
            </div>
            <div className="status-item">
              <div className="status-value rejected">
                {dashboardData.applicationStatus.rejected}
              </div>
              <div className="status-label">Rejected</div>
            </div>
            <div className="status-item">
              <div className="status-value total">
                {dashboardData.applicationStatus.total}
              </div>
              <div className="status-label">Total</div>
            </div>
          </div>
          <button 
            className="card-action-btn"
            onClick={() => navigate('/applications')}
          >
            View All
            <Icons.ArrowRight />
          </button>
        </div>

        {/* Available Products Card */}
        <div className="dashboard-card products">
          <div className="card-icon">
            <Icons.Package />
          </div>
          <h3>Products</h3>
          <div className="summary-value">{dashboardData.availableProductsCount}</div>
          <div className="summary-label">Active financial products available</div>
          <button 
            className="card-action-btn"
            onClick={() => navigate('/products')}
          >
            Browse Products
            <Icons.ArrowRight />
          </button>
        </div>
      </div>

      {/* Account Breakdown */}
      {dashboardData.accountSummary.accounts.length > 0 && (
        <div className="account-breakdown">
          <h3>Account Breakdown</h3>
          <div className="accounts-list">
            {dashboardData.accountSummary.accounts.map((account) => (
              <div key={account.id} className="account-item" onClick={() => navigate(`/accounts`)}>
                <div className="account-info">
                  <div className="account-number">{account.accountNumber}</div>
                  <div className="account-name">{account.accountHolderName}</div>
                  <div className="account-type">{account.accountType}</div>
                </div>
                <div className="account-balance">
                  {formatCurrency(account.balance)}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Recent Transactions */}
      <div className="recent-transactions">
        <div className="section-header">
          <div>
            <h3>Recent Transactions</h3>
            <p className="section-subtitle">Latest {dashboardData.recentTransactions.length} transactions</p>
          </div>
          <button 
            className="view-all-btn"
            onClick={() => navigate('/transactions')}
          >
            View All
            <Icons.ArrowRight />
          </button>
        </div>

        {/* Search and Filter */}
        <div className="transactions-controls">
          <div className="search-box">
            <Icons.Search />
            <input
              type="text"
              placeholder="Search transactions..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="search-input"
            />
            {searchTerm && (
              <button 
                className="clear-search"
                onClick={() => setSearchTerm('')}
                title="Clear search"
              >
                ×
              </button>
            )}
          </div>
          <div className="filter-group">
            <Icons.Filter />
            <select
              value={transactionFilter}
              onChange={(e) => setTransactionFilter(e.target.value)}
              className="filter-select"
            >
              <option value="all">All Types</option>
              {transactionTypes.map((type) => (
                <option key={type} value={type}>{type}</option>
              ))}
            </select>
          </div>
        </div>

        {filteredTransactions.length === 0 ? (
          <div className="empty-state">
            <Icons.FileText />
            <p>No transactions found</p>
            {searchTerm && (
              <button 
                className="clear-filters-btn"
                onClick={() => {
                  setSearchTerm('');
                  setTransactionFilter('all');
                }}
              >
                Clear Filters
              </button>
            )}
          </div>
        ) : (
          <div className="transactions-table">
            <table>
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Type</th>
                  <th>Account</th>
                  <th>Amount</th>
                  <th>Description</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {filteredTransactions.map((transaction) => (
                  <tr 
                    key={transaction.id}
                    className={`transaction-row ${getTransactionTypeColor(transaction.transactionType)}`}
                  >
                    <td>
                      <div className="date-cell">
                        <span className="date-main">{formatDate(transaction.transactionDate)}</span>
                        <span className="date-time">{new Date(transaction.transactionDate).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })}</span>
                      </div>
                    </td>
                    <td>
                      <span className="transaction-type-badge">
                        {transaction.transactionType}
                      </span>
                    </td>
                    <td className="account-cell">
                      {transaction.accountNumber || `#${transaction.accountId}`}
                    </td>
                    <td className={`amount-cell ${getTransactionTypeColor(transaction.transactionType)}`}>
                      <span className="amount-sign">
                        {transaction.transactionType.toLowerCase().includes('deposit') ? '+' : '-'}
                      </span>
                      {formatCurrency(transaction.amount)}
                    </td>
                    <td className="description-cell">{transaction.description}</td>
                    <td>
                      <span className={`status-badge ${transaction.status.toLowerCase()}`}>
                        {transaction.status === 'Completed' ? <Icons.CheckCircle /> : <Icons.Clock />}
                        {transaction.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Quick Actions */}
      <div className="quick-actions">
        <h3>Quick Actions</h3>
        <div className="actions-grid">
          <button 
            className="action-btn"
            onClick={() => navigate('/accounts')}
          >
            <Icons.Wallet />
            <span>Accounts</span>
          </button>
          <button 
            className="action-btn"
            onClick={() => navigate('/transactions')}
          >
            <Icons.TrendingUp />
            <span>Transactions</span>
          </button>
          <button 
            className="action-btn"
            onClick={() => navigate('/applications')}
          >
            <Icons.FileText />
            <span>Applications</span>
          </button>
          <button 
            className="action-btn"
            onClick={() => navigate('/products')}
          >
            <Icons.Package />
            <span>Products</span>
          </button>
        </div>
      </div>
    </div>
  );
}
