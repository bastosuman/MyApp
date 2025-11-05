import './DashboardSkeleton.css';

export function DashboardSkeleton() {
  return (
    <div className="dashboard-skeleton">
      <div className="skeleton-header">
        <div className="skeleton-title"></div>
      </div>
      
      <div className="skeleton-grid">
        <div className="skeleton-card">
          <div className="skeleton-card-header"></div>
          <div className="skeleton-card-value"></div>
          <div className="skeleton-card-label"></div>
          <div className="skeleton-card-button"></div>
        </div>
        <div className="skeleton-card">
          <div className="skeleton-card-header"></div>
          <div className="skeleton-card-value"></div>
          <div className="skeleton-card-label"></div>
          <div className="skeleton-card-button"></div>
        </div>
        <div className="skeleton-card">
          <div className="skeleton-card-header"></div>
          <div className="skeleton-card-value"></div>
          <div className="skeleton-card-label"></div>
          <div className="skeleton-card-button"></div>
        </div>
      </div>

      <div className="skeleton-table">
        <div className="skeleton-table-header">
          <div className="skeleton-table-header-item"></div>
          <div className="skeleton-table-header-item"></div>
          <div className="skeleton-table-header-item"></div>
          <div className="skeleton-table-header-item"></div>
        </div>
        {[1, 2, 3, 4, 5].map((i) => (
          <div key={i} className="skeleton-table-row">
            <div className="skeleton-table-cell"></div>
            <div className="skeleton-table-cell"></div>
            <div className="skeleton-table-cell"></div>
            <div className="skeleton-table-cell"></div>
          </div>
        ))}
      </div>
    </div>
  );
}

