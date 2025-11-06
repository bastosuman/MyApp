import { useState, useEffect } from 'react';
import { transfersService } from '../services/transfers.service';
import { accountsService } from '../services/accounts.service';
import { ScheduledTransfer, CreateScheduledTransfer, Account } from '../types';
import './ScheduledTransfers.css';

export function ScheduledTransfers() {
  const [scheduledTransfers, setScheduledTransfers] = useState<ScheduledTransfer[]>([]);
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [filterAccountId, setFilterAccountId] = useState<number | ''>('');
  const [filterStatus, setFilterStatus] = useState<string>('');

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    loadScheduledTransfers();
  }, [filterAccountId, filterStatus]);

  const loadData = async () => {
    try {
      setLoading(true);
      const [transfersRes, accountsRes] = await Promise.all([
        transfersService.getScheduled(),
        accountsService.getAll()
      ]);
      
      if (transfersRes.success && transfersRes.data) {
        setScheduledTransfers(transfersRes.data);
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

  const loadScheduledTransfers = async () => {
    try {
      const accountId = filterAccountId ? Number(filterAccountId) : undefined;
      const status = filterStatus || undefined;
      const response = await transfersService.getScheduled(accountId, status);
      
      if (response.success && response.data) {
        setScheduledTransfers(response.data);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load scheduled transfers');
    }
  };

  const handleCreate = async (transfer: CreateScheduledTransfer) => {
    try {
      const response = await transfersService.createScheduled(transfer);
      if (response.success) {
        setShowCreateForm(false);
        loadScheduledTransfers();
        alert('Scheduled transfer created successfully!');
      } else {
        alert(response.message || 'Failed to create scheduled transfer');
      }
    } catch (err: any) {
      alert(err.message || 'Failed to create scheduled transfer');
    }
  };

  const handleCancel = async (id: number) => {
    if (!confirm('Are you sure you want to cancel this scheduled transfer?')) return;
    
    try {
      const response = await transfersService.cancelScheduled(id);
      if (response.success) {
        loadScheduledTransfers();
        alert('Scheduled transfer cancelled successfully');
      } else {
        alert(response.message || 'Failed to cancel scheduled transfer');
      }
    } catch (err: any) {
      alert(err.message || 'Failed to cancel scheduled transfer');
    }
  };

  const handlePause = async (id: number) => {
    try {
      const response = await transfersService.pauseScheduled(id);
      if (response.success) {
        loadScheduledTransfers();
        alert('Scheduled transfer paused successfully');
      } else {
        alert(response.message || 'Failed to pause scheduled transfer');
      }
    } catch (err: any) {
      alert(err.message || 'Failed to pause scheduled transfer');
    }
  };

  const handleResume = async (id: number) => {
    try {
      const response = await transfersService.resumeScheduled(id);
      if (response.success) {
        loadScheduledTransfers();
        alert('Scheduled transfer resumed successfully');
      } else {
        alert(response.message || 'Failed to resume scheduled transfer');
      }
    } catch (err: any) {
      alert(err.message || 'Failed to resume scheduled transfer');
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
      case 'active':
        return 'status-badge active';
      case 'paused':
        return 'status-badge paused';
      case 'completed':
        return 'status-badge completed';
      case 'cancelled':
        return 'status-badge cancelled';
      default:
        return 'status-badge';
    }
  };

  if (loading) {
    return <div className="loading">Loading scheduled transfers...</div>;
  }

  return (
    <div className="scheduled-transfers-container">
      <div className="scheduled-transfers-header">
        <h2>Scheduled Transfers</h2>
        <button 
          className="btn btn-primary"
          onClick={() => setShowCreateForm(!showCreateForm)}
        >
          {showCreateForm ? 'Cancel' : 'Schedule Transfer'}
        </button>
      </div>

      {error && <div className="error-message">{error}</div>}

      {showCreateForm && (
        <ScheduledTransferForm
          accounts={accounts}
          onCreate={handleCreate}
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
          <option value="Active">Active</option>
          <option value="Paused">Paused</option>
          <option value="Completed">Completed</option>
          <option value="Cancelled">Cancelled</option>
        </select>
      </div>

      <div className="scheduled-transfers-table-container">
        <table className="scheduled-transfers-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>From Account</th>
              <th>To Account</th>
              <th>Amount</th>
              <th>Description</th>
              <th>Recurrence</th>
              <th>Next Execution</th>
              <th>Status</th>
              <th>Executions</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {scheduledTransfers.length === 0 ? (
              <tr>
                <td colSpan={10} className="no-data">No scheduled transfers found</td>
              </tr>
            ) : (
              scheduledTransfers.map(transfer => (
                <tr key={transfer.id}>
                  <td>{transfer.id}</td>
                  <td>{transfer.sourceAccountNumber}</td>
                  <td>{transfer.destinationAccountNumber || 'N/A'}</td>
                  <td className="amount">{formatCurrency(transfer.amount)}</td>
                  <td>{transfer.description}</td>
                  <td>
                    <span className="recurrence-badge">
                      {transfer.recurrenceType}
                      {transfer.recurrenceDay && ` (Day ${transfer.recurrenceDay})`}
                    </span>
                  </td>
                  <td>{transfer.nextExecutionDate ? formatDate(transfer.nextExecutionDate) : 'N/A'}</td>
                  <td>
                    <span className={getStatusBadgeClass(transfer.status)}>
                      {transfer.status}
                    </span>
                  </td>
                  <td>{transfer.executionCount}</td>
                  <td className="actions">
                    {transfer.status === 'Active' && (
                      <button
                        className="btn btn-sm btn-warning"
                        onClick={() => handlePause(transfer.id)}
                      >
                        Pause
                      </button>
                    )}
                    {transfer.status === 'Paused' && (
                      <button
                        className="btn btn-sm btn-primary"
                        onClick={() => handleResume(transfer.id)}
                      >
                        Resume
                      </button>
                    )}
                    {(transfer.status === 'Active' || transfer.status === 'Paused') && (
                      <button
                        className="btn btn-sm btn-danger"
                        onClick={() => handleCancel(transfer.id)}
                      >
                        Cancel
                      </button>
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

interface ScheduledTransferFormProps {
  accounts: Account[];
  onCreate: (transfer: CreateScheduledTransfer) => void;
  onCancel: () => void;
}

function ScheduledTransferForm({
  accounts,
  onCreate,
  onCancel
}: ScheduledTransferFormProps) {
  const [sourceAccountId, setSourceAccountId] = useState<number | ''>('');
  const [destinationAccountId, setDestinationAccountId] = useState<number | ''>('');
  const [destinationAccountNumber, setDestinationAccountNumber] = useState('');
  const [transferType, setTransferType] = useState<'internal' | 'external'>('internal');
  const [amount, setAmount] = useState('');
  const [description, setDescription] = useState('');
  const [scheduledDate, setScheduledDate] = useState('');
  const [recurrenceType, setRecurrenceType] = useState('OneTime');
  const [recurrenceDay, setRecurrenceDay] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!sourceAccountId || !amount || parseFloat(amount) <= 0 || !scheduledDate) {
      alert('Please fill in all required fields');
      return;
    }

    if (transferType === 'internal') {
      if (!destinationAccountId) {
        alert('Please select a destination account');
        return;
      }
    } else {
      if (!destinationAccountNumber) {
        alert('Please enter a destination account number');
        return;
      }
    }

    onCreate({
      sourceAccountId: Number(sourceAccountId),
      destinationAccountId: transferType === 'internal' ? Number(destinationAccountId) : undefined,
      destinationAccountNumber: transferType === 'external' ? destinationAccountNumber : undefined,
      amount: parseFloat(amount),
      description,
      scheduledDate,
      recurrenceType,
      recurrenceDay: recurrenceDay ? Number(recurrenceDay) : undefined
    });
  };

  return (
    <div className="scheduled-transfer-form-container">
      <h3>Schedule New Transfer</h3>
      <form onSubmit={handleSubmit} className="scheduled-transfer-form">
        <div className="form-group">
          <label>Transfer Type</label>
          <div className="radio-group">
            <label>
              <input
                type="radio"
                value="internal"
                checked={transferType === 'internal'}
                onChange={(e) => setTransferType(e.target.value as 'internal' | 'external')}
              />
              Internal
            </label>
            <label>
              <input
                type="radio"
                value="external"
                checked={transferType === 'external'}
                onChange={(e) => setTransferType(e.target.value as 'internal' | 'external')}
              />
              External
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
                {account.accountNumber} - {account.accountHolderName}
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
          <label htmlFor="scheduledDate">Scheduled Date *</label>
          <input
            id="scheduledDate"
            type="datetime-local"
            value={scheduledDate}
            onChange={(e) => setScheduledDate(e.target.value)}
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="recurrenceType">Recurrence Type *</label>
          <select
            id="recurrenceType"
            value={recurrenceType}
            onChange={(e) => setRecurrenceType(e.target.value)}
            required
          >
            <option value="OneTime">One Time</option>
            <option value="Daily">Daily</option>
            <option value="Weekly">Weekly</option>
            <option value="Monthly">Monthly</option>
            <option value="Quarterly">Quarterly</option>
            <option value="Annually">Annually</option>
          </select>
        </div>

        {(recurrenceType === 'Weekly' || recurrenceType === 'Monthly') && (
          <div className="form-group">
            <label htmlFor="recurrenceDay">
              {recurrenceType === 'Weekly' ? 'Day of Week (1-7)' : 'Day of Month (1-31)'}
            </label>
            <input
              id="recurrenceDay"
              type="number"
              min="1"
              max={recurrenceType === 'Weekly' ? 7 : 31}
              value={recurrenceDay}
              onChange={(e) => setRecurrenceDay(e.target.value)}
              placeholder={recurrenceType === 'Weekly' ? '1-7' : '1-31'}
            />
          </div>
        )}

        <div className="form-actions">
          <button type="submit" className="btn btn-primary">Schedule Transfer</button>
          <button type="button" className="btn btn-secondary" onClick={onCancel}>Cancel</button>
        </div>
      </form>
    </div>
  );
}


