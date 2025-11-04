import { useState, useEffect } from 'react';
import { accountsService } from '../services/accounts.service';
import { Account, CreateAccount, UpdateAccount } from '../types';
import './Accounts.css';

export function Accounts() {
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingAccount, setEditingAccount] = useState<Account | null>(null);

  useEffect(() => {
    loadAccounts();
  }, []);

  const loadAccounts = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await accountsService.getAll();
      if (response.success && response.data) {
        setAccounts(response.data);
      } else {
        setError(response.message || 'Failed to load accounts');
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load accounts');
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = async (account: CreateAccount) => {
    try {
      const response = await accountsService.create(account);
      if (response.success) {
        setShowCreateForm(false);
        loadAccounts();
      } else {
        alert(response.message || 'Failed to create account');
      }
    } catch (err: any) {
      alert(err.message || 'Failed to create account');
    }
  };

  const handleUpdate = async (id: number, account: UpdateAccount) => {
    try {
      const response = await accountsService.update(id, account);
      if (response.success) {
        setEditingAccount(null);
        loadAccounts();
      } else {
        alert(response.message || 'Failed to update account');
      }
    } catch (err: any) {
      alert(err.message || 'Failed to update account');
    }
  };

  if (loading) {
    return <div className="loading">Loading accounts...</div>;
  }

  return (
    <div className="accounts-container">
      <div className="accounts-header">
        <h2>Accounts</h2>
        <button onClick={() => setShowCreateForm(true)} className="btn btn-primary">
          Create Account
        </button>
      </div>

      {error && <div className="error-message">{error}</div>}

      {showCreateForm && (
        <CreateAccountForm
          onSubmit={handleCreate}
          onCancel={() => setShowCreateForm(false)}
        />
      )}

      {editingAccount && (
        <EditAccountForm
          account={editingAccount}
          onSubmit={(account) => handleUpdate(editingAccount.id, account)}
          onCancel={() => setEditingAccount(null)}
        />
      )}

      <div className="accounts-grid">
        {accounts.map((account) => (
          <AccountCard
            key={account.id}
            account={account}
            onEdit={() => setEditingAccount(account)}
          />
        ))}
      </div>

      {accounts.length === 0 && !loading && (
        <div className="empty-state">No accounts found</div>
      )}
    </div>
  );
}

function AccountCard({ account, onEdit }: { account: Account; onEdit: () => void }) {
  return (
    <div className="account-card">
      <div className="account-card-header">
        <h3>{account.accountNumber}</h3>
        <span className={`status-badge ${account.isActive ? 'active' : 'inactive'}`}>
          {account.isActive ? 'Active' : 'Inactive'}
        </span>
      </div>
      <div className="account-card-body">
        <p><strong>Holder:</strong> {account.accountHolderName}</p>
        <p><strong>Type:</strong> {account.accountType}</p>
        <p><strong>Balance:</strong> ${account.balance.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</p>
        <p><strong>Created:</strong> {new Date(account.createdDate).toLocaleDateString()}</p>
      </div>
      <div className="account-card-actions">
        <button onClick={onEdit} className="btn btn-secondary">Edit</button>
      </div>
    </div>
  );
}

function CreateAccountForm({
  onSubmit,
  onCancel,
}: {
  onSubmit: (account: CreateAccount) => void;
  onCancel: () => void;
}) {
  const [formData, setFormData] = useState<CreateAccount>({
    accountNumber: '',
    accountHolderName: '',
    balance: 0,
    accountType: 'Savings',
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  return (
    <div className="modal-overlay">
      <div className="modal">
        <h3>Create New Account</h3>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Account Number</label>
            <input
              type="text"
              value={formData.accountNumber}
              onChange={(e) => setFormData({ ...formData, accountNumber: e.target.value })}
              required
              maxLength={50}
            />
          </div>
          <div className="form-group">
            <label>Account Holder Name</label>
            <input
              type="text"
              value={formData.accountHolderName}
              onChange={(e) => setFormData({ ...formData, accountHolderName: e.target.value })}
              required
              maxLength={200}
            />
          </div>
          <div className="form-group">
            <label>Initial Balance</label>
            <input
              type="number"
              step="0.01"
              min="0"
              value={formData.balance}
              onChange={(e) => setFormData({ ...formData, balance: parseFloat(e.target.value) || 0 })}
              required
            />
          </div>
          <div className="form-group">
            <label>Account Type</label>
            <select
              value={formData.accountType}
              onChange={(e) => setFormData({ ...formData, accountType: e.target.value })}
              required
            >
              <option value="Savings">Savings</option>
              <option value="Checking">Checking</option>
              <option value="Investment">Investment</option>
            </select>
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

function EditAccountForm({
  account,
  onSubmit,
  onCancel,
}: {
  account: Account;
  onSubmit: (account: UpdateAccount) => void;
  onCancel: () => void;
}) {
  const [formData, setFormData] = useState<UpdateAccount>({
    accountHolderName: account.accountHolderName,
    balance: account.balance,
    accountType: account.accountType,
    isActive: account.isActive,
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  return (
    <div className="modal-overlay">
      <div className="modal">
        <h3>Edit Account {account.accountNumber}</h3>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Account Holder Name</label>
            <input
              type="text"
              value={formData.accountHolderName}
              onChange={(e) => setFormData({ ...formData, accountHolderName: e.target.value })}
              required
              maxLength={200}
            />
          </div>
          <div className="form-group">
            <label>Balance</label>
            <input
              type="number"
              step="0.01"
              min="0"
              value={formData.balance}
              onChange={(e) => setFormData({ ...formData, balance: parseFloat(e.target.value) || 0 })}
              required
            />
          </div>
          <div className="form-group">
            <label>Account Type</label>
            <select
              value={formData.accountType}
              onChange={(e) => setFormData({ ...formData, accountType: e.target.value })}
              required
            >
              <option value="Savings">Savings</option>
              <option value="Checking">Checking</option>
              <option value="Investment">Investment</option>
            </select>
          </div>
          <div className="form-group">
            <label>
              <input
                type="checkbox"
                checked={formData.isActive}
                onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
              />
              Active
            </label>
          </div>
          <div className="form-actions">
            <button type="submit" className="btn btn-primary">Update</button>
            <button type="button" onClick={onCancel} className="btn btn-secondary">Cancel</button>
          </div>
        </form>
      </div>
    </div>
  );
}

