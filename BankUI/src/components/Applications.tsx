import { useState, useEffect } from 'react';
import { applicationsService } from '../services/applications.service';
import { accountsService } from '../services/accounts.service';
import { productsService } from '../services/products.service';
import { Application, CreateApplication, UpdateApplicationStatus, Account, Product } from '../types';
import './Applications.css';

export function Applications() {
  const [applications, setApplications] = useState<Application[]>([]);
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [statusFilter, setStatusFilter] = useState<string>('');

  useEffect(() => {
    loadApplications();
    loadAccounts();
    loadProducts();
  }, [statusFilter]);

  const loadApplications = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await applicationsService.getAll(statusFilter || undefined);
      if (response.success && response.data) {
        setApplications(response.data);
      } else {
        setError(response.message || 'Failed to load applications');
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load applications');
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
      // Silently fail
    }
  };

  const loadProducts = async () => {
    try {
      const response = await productsService.getAll();
      if (response.success && response.data) {
        setProducts(response.data);
      }
    } catch (err) {
      // Silently fail
    }
  };

  const handleCreate = async (application: CreateApplication) => {
    try {
      const response = await applicationsService.create(application);
      if (response.success) {
        setShowCreateForm(false);
        loadApplications();
      } else {
        alert(response.message || 'Failed to create application');
      }
    } catch (err: any) {
      alert(err.message || 'Failed to create application');
    }
  };

  const handleUpdateStatus = async (id: number, update: UpdateApplicationStatus) => {
    try {
      const response = await applicationsService.updateStatus(id, update);
      if (response.success) {
        loadApplications();
      } else {
        alert(response.message || 'Failed to update application status');
      }
    } catch (err: any) {
      alert(err.message || 'Failed to update application status');
    }
  };

  if (loading) {
    return <div className="loading">Loading applications...</div>;
  }

  return (
    <div className="applications-container">
      <div className="applications-header">
        <h2>Applications</h2>
        <div className="header-actions">
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="status-filter"
          >
            <option value="">All Statuses</option>
            <option value="Pending">Pending</option>
            <option value="Approved">Approved</option>
            <option value="Rejected">Rejected</option>
          </select>
          <button onClick={() => setShowCreateForm(true)} className="btn btn-primary">
            New Application
          </button>
        </div>
      </div>

      {error && <div className="error-message">{error}</div>}

      {showCreateForm && (
        <CreateApplicationForm
          accounts={accounts}
          products={products}
          onSubmit={handleCreate}
          onCancel={() => setShowCreateForm(false)}
        />
      )}

      <div className="applications-grid">
        {applications.map((application) => (
          <ApplicationCard
            key={application.id}
            application={application}
            onUpdateStatus={(update) => handleUpdateStatus(application.id, update)}
          />
        ))}
      </div>

      {applications.length === 0 && !loading && (
        <div className="empty-state">No applications found</div>
      )}
    </div>
  );
}

function ApplicationCard({
  application,
  onUpdateStatus,
}: {
  application: Application;
  onUpdateStatus: (update: UpdateApplicationStatus) => void;
}) {
  const [showUpdateForm, setShowUpdateForm] = useState(false);

  return (
    <>
      <div className="application-card">
        <div className="application-card-header">
          <div>
            <h3>
              {application.productName || `Product #${application.productId}`}
            </h3>
            <p className="account-info">
              Account: {application.accountNumber || `#${application.accountId}`}
            </p>
          </div>
          <span className={`status-badge status-${application.status.toLowerCase()}`}>
            {application.status}
          </span>
        </div>
        <div className="application-card-body">
          <div className="application-detail">
            <span className="detail-label">Requested Amount:</span>
            <span className="detail-value">
              ${application.requestedAmount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
            </span>
          </div>
          <div className="application-detail">
            <span className="detail-label">Application Date:</span>
            <span className="detail-value">
              {new Date(application.applicationDate).toLocaleDateString()}
            </span>
          </div>
          {application.decisionDate && (
            <div className="application-detail">
              <span className="detail-label">Decision Date:</span>
              <span className="detail-value">
                {new Date(application.decisionDate).toLocaleDateString()}
              </span>
            </div>
          )}
          {application.notes && (
            <div className="application-notes">
              <strong>Notes:</strong> {application.notes}
            </div>
          )}
        </div>
        <div className="application-card-actions">
          {application.status === 'Pending' && (
            <button onClick={() => setShowUpdateForm(true)} className="btn btn-secondary">
              Update Status
            </button>
          )}
        </div>
      </div>

      {showUpdateForm && (
        <UpdateStatusForm
          application={application}
          onSubmit={onUpdateStatus}
          onCancel={() => setShowUpdateForm(false)}
        />
      )}
    </>
  );
}

function CreateApplicationForm({
  accounts,
  products,
  onSubmit,
  onCancel,
}: {
  accounts: Account[];
  products: Product[];
  onSubmit: (application: CreateApplication) => void;
  onCancel: () => void;
}) {
  const [formData, setFormData] = useState<CreateApplication>({
    accountId: accounts[0]?.id || 0,
    productId: products[0]?.id || 0,
    requestedAmount: 0,
    notes: '',
  });

  const selectedProduct = products.find((p) => p.id === formData.productId);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  return (
    <div className="modal-overlay">
      <div className="modal">
        <h3>Create New Application</h3>
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
            <label>Product</label>
            <select
              value={formData.productId}
              onChange={(e) => setFormData({ ...formData, productId: parseInt(e.target.value) })}
              required
            >
              <option value="">Select a product</option>
              {products.filter((p) => p.isActive).map((product) => (
                <option key={product.id} value={product.id}>
                  {product.name} ({product.interestRate}% - ${product.minAmount.toLocaleString()} - ${product.maxAmount.toLocaleString()})
                </option>
              ))}
            </select>
          </div>
          {selectedProduct && (
            <div className="product-info">
              <p>
                Amount range: ${selectedProduct.minAmount.toLocaleString()} - ${selectedProduct.maxAmount.toLocaleString()}
              </p>
            </div>
          )}
          <div className="form-group">
            <label>Requested Amount</label>
            <input
              type="number"
              step="0.01"
              min={selectedProduct?.minAmount || 0}
              max={selectedProduct?.maxAmount || undefined}
              value={formData.requestedAmount}
              onChange={(e) => setFormData({ ...formData, requestedAmount: parseFloat(e.target.value) || 0 })}
              required
            />
          </div>
          <div className="form-group">
            <label>Notes (Optional)</label>
            <textarea
              value={formData.notes || ''}
              onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
              rows={3}
              maxLength={1000}
            />
          </div>
          <div className="form-actions">
            <button type="submit" className="btn btn-primary">Submit</button>
            <button type="button" onClick={onCancel} className="btn btn-secondary">Cancel</button>
          </div>
        </form>
      </div>
    </div>
  );
}

function UpdateStatusForm({
  application,
  onSubmit,
  onCancel,
}: {
  application: Application;
  onSubmit: (update: UpdateApplicationStatus) => void;
  onCancel: () => void;
}) {
  const [formData, setFormData] = useState<UpdateApplicationStatus>({
    status: 'Approved',
    notes: '',
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
    onCancel();
  };

  return (
    <div className="modal-overlay">
      <div className="modal">
        <h3>Update Application Status</h3>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Status</label>
            <select
              value={formData.status}
              onChange={(e) => setFormData({ ...formData, status: e.target.value })}
              required
            >
              <option value="Approved">Approved</option>
              <option value="Rejected">Rejected</option>
            </select>
          </div>
          <div className="form-group">
            <label>Notes (Optional)</label>
            <textarea
              value={formData.notes || ''}
              onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
              rows={3}
              maxLength={1000}
            />
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

