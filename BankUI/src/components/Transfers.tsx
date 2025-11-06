import { useState, useEffect } from 'react';
import { transfersService } from '../services/transfers.service';
import { accountsService } from '../services/accounts.service';
import { Transfer, CreateInternalTransfer, CreateExternalTransfer, Account } from '../types';
import './Transfers.css';

export function Transfers() {
  const [transfers, setTransfers] = useState<Transfer[]>([]);
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [transferType, setTransferType] = useState<'internal' | 'external'>('internal');
  const [filterAccountId, setFilterAccountId] = useState<number | ''>('');
  const [filterStatus, setFilterStatus] = useState<string>('');

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    loadTransfers();
  }, [filterAccountId, filterStatus]);

  const loadData = async () => {
    try {
      setLoading(true);
      const [transfersRes, accountsRes] = await Promise.all([
        transfersService.getAll(),
        accountsService.getAll()
      ]);
      
      if (transfersRes.success && transfersRes.data) {
        setTransfers(transfersRes.data);
      }
      
      if (accountsRes.success && accountsRes.data) {
        setAccounts(accountsRes.data.filter(a => a.isActive));
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  const loadTransfers = async () => {
    try {
      const accountId = filterAccountId ? Number(filterAccountId) : undefined;
      const status = filterStatus || undefined;
      const response = await transfersService.getAll(accountId, status);
      
      if (response.success && response.data) {
        setTransfers(response.data);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load transfers');
    }
  };

  const handleCreateInternal = async (transfer: CreateInternalTransfer) => {
    try {
      const response = await transfersService.createInternal(transfer);
      if (response.success) {
        setShowCreateForm(false);
        loadTransfers();
        alert('Transfer created successfully!');
      } else {
        alert(response.message || 'Failed to create transfer');
      }
    } catch (err: any) {
      alert(err.message || 'Failed to create transfer');
    }
  };

  const handleCreateExternal = async (transfer: CreateExternalTransfer) => {
    try {
      const response = await transfersService.createExternal(transfer);
      if (response.success) {
        setShowCreateForm(false);
        loadTransfers();
        alert('Transfer created successfully!');
      } else {
        alert(response.message || 'Failed to create transfer');
      }
    } catch (err: any) {
      alert(err.message || 'Failed to create transfer');
    }
  };

  const handleCancel = async (id: number) => {
    if (!confirm('Are you sure you want to cancel this transfer?')) return;
    
    try {
      const response = await transfersService.cancel(id);
      if (response.success) {
        loadTransfers();
        alert('Transfer cancelled successfully');
      } else {
        alert(response.message || 'Failed to cancel transfer');
      }
    } catch (err: any) {
      alert(err.message || 'Failed to cancel transfer');
    }
  };

  const handleRetry = async (id: number) => {
    try {
      const response = await transfersService.retry(id);
      if (response.success) {
        loadTransfers();
        alert('Transfer retried successfully');
      } else {
        alert(response.message || 'Failed to retry transfer');
      }
    } catch (err: any) {
      alert(err.message || 'Failed to retry transfer');
    }
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };

  const getStatusBadgeClass = (status: string) => {
    switch (status.toLowerCase()) {
      case 'completed':
        return 'status-badge completed';
      case 'pending':
      case 'processing':
        return 'status-badge pending';
      case 'failed':
        return 'status-badge failed';
      case 'cancelled':
        return 'status-badge cancelled';
      default:
        return 'status-badge';
    }
  };

  if (loading) {
    return <div className="loading">Loading transfers...</div>;
  }

  return (
    <div className="transfers-container">
      <div className="transfers-header">
        <h2>Transfers</h2>
        <button 
          className="btn btn-primary"
          onClick={() => setShowCreateForm(!showCreateForm)}
        >
          {showCreateForm ? 'Cancel' : 'Create Transfer'}
        </button>
      </div>

      {error && <div className="error-message">{error}</div>}

      {showCreateForm && (
        <TransferForm
          accounts={accounts}
          transferType={transferType}
          onTransferTypeChange={setTransferType}
          onCreateInternal={handleCreateInternal}
          onCreateExternal={handleCreateExternal}
          onCancel={() => setShowCreateForm(false)}
        />
      )}

      <div className="filters">
        <select
          value={filterAccountId}
          onChange={(e) => setFilterAccountId(e.target.value ? Number(e.target.value) : '')}
          className="filter-select"
        >
          <option value="">All Accounts</option>
          {accounts.map(account => (
            <option key={account.id} value={account.id}>
              {account.accountNumber} - {account.accountHolderName}
            </option>
          ))}
        </select>

        <select
          value={filterStatus}
          onChange={(e) => setFilterStatus(e.target.value)}
          className="filter-select"
        >
          <option value="">All Statuses</option>
          <option value="Pending">Pending</option>
          <option value="Processing">Processing</option>
          <option value="Completed">Completed</option>
          <option value="Failed">Failed</option>
          <option value="Cancelled">Cancelled</option>
        </select>
      </div>

      <div className="transfers-table-container">
        <table className="transfers-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Type</th>
              <th>From Account</th>
              <th>To Account</th>
              <th>Amount</th>
              <th>Description</th>
              <th>Status</th>
              <th>Date</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {transfers.length === 0 ? (
              <tr>
                <td colSpan={9} className="no-data">No transfers found</td>
              </tr>
            ) : (
              transfers.map(transfer => (
                <tr key={transfer.id}>
                  <td>{transfer.id}</td>
                  <td>
                    <span className={`transfer-type ${transfer.transferType.toLowerCase()}`}>
                      {transfer.transferType}
                    </span>
                  </td>
                  <td>{transfer.sourceAccountNumber}</td>
                  <td>{transfer.destinationAccountNumber || 'N/A'}</td>
                  <td className="amount">{formatCurrency(transfer.amount)}</td>
                  <td>{transfer.description}</td>
                  <td>
                    <span className={getStatusBadgeClass(transfer.status)}>
                      {transfer.status}
                    </span>
                  </td>
                  <td>{formatDate(transfer.transferDate)}</td>
                  <td className="actions">
                    {transfer.status === 'Pending' && (
                      <button
                        className="btn btn-sm btn-danger"
                        onClick={() => handleCancel(transfer.id)}
                      >
                        Cancel
                      </button>
                    )}
                    {transfer.status === 'Failed' && (
                      <button
                        className="btn btn-sm btn-primary"
                        onClick={() => handleRetry(transfer.id)}
                      >
                        Retry
                      </button>
                    )}
                    {transfer.failureReason && (
                      <span className="failure-reason" title={transfer.failureReason}>
                        ⚠️
                      </span>
                    )}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

interface TransferFormProps {
  accounts: Account[];
  transferType: 'internal' | 'external';
  onTransferTypeChange: (type: 'internal' | 'external') => void;
  onCreateInternal: (transfer: CreateInternalTransfer) => void;
  onCreateExternal: (transfer: CreateExternalTransfer) => void;
  onCancel: () => void;
}

function TransferForm({
  accounts,
  transferType,
  onTransferTypeChange,
  onCreateInternal,
  onCreateExternal,
  onCancel
}: TransferFormProps) {
  const [sourceAccountId, setSourceAccountId] = useState<number | ''>('');
  const [destinationAccountId, setDestinationAccountId] = useState<number | ''>('');
  const [destinationAccountNumber, setDestinationAccountNumber] = useState('');
  const [amount, setAmount] = useState('');
  const [description, setDescription] = useState('');
  const [scheduledDate, setScheduledDate] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!sourceAccountId || !amount || parseFloat(amount) <= 0) {
      alert('Please fill in all required fields');
      return;
    }

    if (transferType === 'internal') {
      if (!destinationAccountId) {
        alert('Please select a destination account');
        return;
      }
      onCreateInternal({
        sourceAccountId: Number(sourceAccountId),
        destinationAccountId: Number(destinationAccountId),
        amount: parseFloat(amount),
        description,
        scheduledDate: scheduledDate || undefined
      });
    } else {
      if (!destinationAccountNumber) {
        alert('Please enter a destination account number');
        return;
      }
      onCreateExternal({
        sourceAccountId: Number(sourceAccountId),
        destinationAccountNumber,
        amount: parseFloat(amount),
        description,
        scheduledDate: scheduledDate || undefined
      });
    }
  };

  return (
    <div className="transfer-form-container">
      <h3>Create New Transfer</h3>
      <form onSubmit={handleSubmit} className="transfer-form">
        <div className="form-group">
          <label>Transfer Type</label>
          <div className="radio-group">
            <label>
              <input
                type="radio"
                value="internal"
                checked={transferType === 'internal'}
                onChange={(e) => onTransferTypeChange(e.target.value as 'internal' | 'external')}
              />
              Internal (Between My Accounts)
            </label>
            <label>
              <input
                type="radio"
                value="external"
                checked={transferType === 'external'}
                onChange={(e) => onTransferTypeChange(e.target.value as 'internal' | 'external')}
              />
              External (To Another Account)
            </label>
          </div>
        </div>

        <div className="form-group">
          <label htmlFor="sourceAccount">From Account *</label>
          <select
            id="sourceAccount"
            value={sourceAccountId}
            onChange={(e) => setSourceAccountId(e.target.value ? Number(e.target.value) : '')}
            required
          >
            <option value="">Select account</option>
            {accounts.map(account => (
              <option key={account.id} value={account.id}>
                {account.accountNumber} - {account.accountHolderName} ({formatCurrency(account.balance)})
              </option>
            ))}
          </select>
        </div>

        {transferType === 'internal' ? (
          <div className="form-group">
            <label htmlFor="destinationAccount">To Account *</label>
            <select
              id="destinationAccount"
              value={destinationAccountId}
              onChange={(e) => setDestinationAccountId(e.target.value ? Number(e.target.value) : '')}
              required
            >
              <option value="">Select account</option>
              {accounts
                .filter(acc => acc.id !== sourceAccountId)
                .map(account => (
                  <option key={account.id} value={account.id}>
                    {account.accountNumber} - {account.accountHolderName}
                  </option>
                ))}
            </select>
          </div>
        ) : (
          <div className="form-group">
            <label htmlFor="destinationAccountNumber">To Account Number *</label>
            <input
              id="destinationAccountNumber"
              type="text"
              value={destinationAccountNumber}
              onChange={(e) => setDestinationAccountNumber(e.target.value)}
              placeholder="Enter destination account number"
              required
            />
          </div>
        )}

        <div className="form-group">
          <label htmlFor="amount">Amount *</label>
          <input
            id="amount"
            type="number"
            step="0.01"
            min="0.01"
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            placeholder="0.00"
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="description">Description</label>
          <input
            id="description"
            type="text"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="Transfer description"
          />
        </div>

        <div className="form-group">
          <label htmlFor="scheduledDate">Scheduled Date (Optional)</label>
          <input
            id="scheduledDate"
            type="datetime-local"
            value={scheduledDate}
            onChange={(e) => setScheduledDate(e.target.value)}
          />
        </div>

        <div className="form-actions">
          <button type="submit" className="btn btn-primary">Create Transfer</button>
          <button type="button" className="btn btn-secondary" onClick={onCancel}>Cancel</button>
        </div>
      </form>
    </div>
  );

  function formatCurrency(amount: number) {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }
}


