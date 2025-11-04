import { useState, useEffect } from 'react';
import { productsService } from '../services/products.service';
import { Product } from '../types';
import './Products.css';

export function Products() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [includeInactive, setIncludeInactive] = useState(false);

  useEffect(() => {
    loadProducts();
  }, [includeInactive]);

  const loadProducts = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await productsService.getAll(includeInactive);
      if (response.success && response.data) {
        setProducts(response.data);
      } else {
        setError(response.message || 'Failed to load products');
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load products');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return <div className="loading">Loading products...</div>;
  }

  return (
    <div className="products-container">
      <div className="products-header">
        <h2>Financial Products</h2>
        <label className="toggle-inactive">
          <input
            type="checkbox"
            checked={includeInactive}
            onChange={(e) => setIncludeInactive(e.target.checked)}
          />
          Show Inactive Products
        </label>
      </div>

      {error && <div className="error-message">{error}</div>}

      <div className="products-grid">
        {products.map((product) => (
          <ProductCard key={product.id} product={product} />
        ))}
      </div>

      {products.length === 0 && !loading && (
        <div className="empty-state">No products found</div>
      )}
    </div>
  );
}

function ProductCard({ product }: { product: Product }) {
  return (
    <div className={`product-card ${!product.isActive ? 'inactive' : ''}`}>
      <div className="product-card-header">
        <h3>{product.name}</h3>
        <span className={`status-badge ${product.isActive ? 'active' : 'inactive'}`}>
          {product.isActive ? 'Active' : 'Inactive'}
        </span>
      </div>
      <div className="product-card-body">
        <p className="product-type">{product.productType}</p>
        <div className="product-details">
          <div className="detail-item">
            <span className="detail-label">Interest Rate:</span>
            <span className="detail-value">{product.interestRate}%</span>
          </div>
          <div className="detail-item">
            <span className="detail-label">Amount Range:</span>
            <span className="detail-value">
              ${product.minAmount.toLocaleString()} - ${product.maxAmount.toLocaleString()}
            </span>
          </div>
        </div>
        <p className="product-description">{product.description}</p>
      </div>
    </div>
  );
}

