import { useState, useEffect } from 'react';
import { transactionsService } from '../services/transactions.service';
import { accountsService } from '../services/accounts.service';
import { Transaction, CreateTransaction, Account } from '../types';
import './Transactions.css';

export function Transactions() {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(50);

  useEffect(() => {
    loadTransactions();
    loadAccounts();
  }, [page]);

  const loadTransactions = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await transactionsService.getAll(page, pageSize);
      if (response.success && response.data) {
        setTransactions(response.data);
      } else {
        setError(response.message || 'Failed to load transactions');
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load transactions');
    } finally {
      setLoading(false);
    }
  };

  const loadAccounts = async () => {
    try {
      const response = await accountsService.getAll();
      if (response.success && response.data) {
        setAccounts(response.data);
      }
    } catch (err) {
      // Silently fail - accounts are optional for transaction list
    }
  };

  const handleCreate = async (transaction: CreateTransaction) => {
    try {
      const response = await transactionsService.create(transaction);
      if (response.success) {
        setShowCreateForm(false);
        loadTransactions();
      } else {
        alert(response.message || 'Failed to create transaction');
      }
    } catch (err: any) {
      alert(err.message || 'Failed to create transaction');
    }
  };

  if (loading) {
    return <div className="loading">Loading transactions...</div>;
  }

  return (
    <div className="transactions-container">
      <div className="transactions-header">
        <h2>Transactions</h2>
        <button onClick={() => setShowCreateForm(true)} className="btn btn-primary">
          Create Transaction
        </button>
      </div>

      {error && <div className="error-message">{error}</div>}

      {showCreateForm && (
        <CreateTransactionForm
          accounts={accounts}
          onSubmit={handleCreate}
          onCancel={() => setShowCreateForm(false)}
        />
      )}

      <div className="transactions-table-container">
        <table className="transactions-table">
          <thead>
            <tr>
              <th>Date</th>
              <th>Account</th>
              <th>Type</th>
              <th>Amount</th>
              <th>Description</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {transactions.map((transaction) => (
              <TransactionRow key={transaction.id} transaction={transaction} />
            ))}
          </tbody>
        </table>
      </div>

      {transactions.length === 0 && !loading && (
        <div className="empty-state">No transactions found</div>
      )}

      <div className="pagination">
        <button
          onClick={() => setPage(Math.max(1, page - 1))}
          disabled={page === 1}
          className="btn btn-secondary"
        >
          Previous
        </button>
        <span>Page {page}</span>
        <button
          onClick={() => setPage(page + 1)}
          disabled={transactions.length < pageSize}
          className="btn btn-secondary"
        >
          Next
        </button>
      </div>
    </div>
  );
}

function TransactionRow({ transaction }: { transaction: Transaction }) {
  const amountClass = transaction.transactionType === 'Deposit' ? 'positive' : 'negative';
  return (
    <tr>
      <td>{new Date(transaction.transactionDate).toLocaleString()}</td>
      <td>{transaction.accountNumber || `Account #${transaction.accountId}`}</td>
      <td>
        <span className={`transaction-type ${transaction.transactionType.toLowerCase()}`}>
          {transaction.transactionType}
        </span>
      </td>
      <td className={amountClass}>
        {transaction.transactionType === 'Deposit' ? '+' : '-'}
        ${transaction.amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
      </td>
      <td>{transaction.description}</td>
      <td>
        <span className={`status-badge ${transaction.status.toLowerCase()}`}>
          {transaction.status}
        </span>
      </td>
    </tr>
  );
}

function CreateTransactionForm({
  accounts,
  onSubmit,
  onCancel,
}: {
  accounts: Account[];
  onSubmit: (transaction: CreateTransaction) => void;
  onCancel: () => void;
}) {
  const [formData, setFormData] = useState<CreateTransaction>({
    accountId: accounts[0]?.id || 0,
    transactionType: 'Deposit',
    amount: 0,
    description: '',
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  return (
    <div className="modal-overlay">
      <div className="modal">
        <h3>Create New Transaction</h3>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Account</label>
            <select
              value={formData.accountId}
              onChange={(e) => setFormData({ ...formData, accountId: parseInt(e.target.value) })}
              required
            >
              <option value="">Select an account</option>
              {accounts.map((account) => (
                <option key={account.id} value={account.id}>
                  {account.accountNumber} - {account.accountHolderName}
                </option>
              ))}
            </select>
          </div>
          <div className="form-group">
            <label>Transaction Type</label>
            <select
              value={formData.transactionType}
              onChange={(e) => setFormData({ ...formData, transactionType: e.target.value })}
              required
            >
              <option value="Deposit">Deposit</option>
              <option value="Withdrawal">Withdrawal</option>
              <option value="Transfer">Transfer</option>
            </select>
          </div>
          <div className="form-group">
            <label>Amount</label>
            <input
              type="number"
              step="0.01"
              min="0.01"
              value={formData.amount}
              onChange={(e) => setFormData({ ...formData, amount: parseFloat(e.target.value) || 0 })}
              required
            />
          </div>
          <div className="form-group">
            <label>Description</label>
            <input
              type="text"
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              maxLength={500}
            />
          </div>
          <div className="form-actions">
            <button type="submit" className="btn btn-primary">Create</button>
            <button type="button" onClick={onCancel} className="btn btn-secondary">Cancel</button>
          </div>
        </form>
      </div>
    </div>
  );
}

