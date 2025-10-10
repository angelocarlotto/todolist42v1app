import React from 'react';
import { useApp } from '../context/AppContext';
import './Navbar.css';

function Navbar() {
  const { user, logout, notifications } = useApp();

  const handleLogout = () => {
    if (window.confirm('Are you sure you want to logout?')) {
      logout();
    }
  };

  return (
    <nav className="navbar">
      <div className="navbar-brand">
        <h1>TaskFlow</h1>
      </div>
      {/* Debug info: show user and auth state */}
      <div style={{ color: 'red', fontSize: '0.8em' }}>
        DEBUG: user = {JSON.stringify(user)}
      </div>
      <div className="navbar-content">
        {notifications.length > 0 && (
          <div className="notifications">
            <span className="notification-badge">{notifications.length}</span>
            🔔
          </div>
        )}
        
        <div className="user-info" style={{ border: '2px solid red', background: '#fffbe6', padding: '4px', borderRadius: '6px' }}>
          <span>Welcome, {user?.username}</span>
          <button className="btn btn-link" style={{ border: '2px solid blue', background: '#e6f7ff', marginLeft: '8px' }} onClick={handleLogout}>
            Logout
          </button>
        </div>
      </div>
    </nav>
  );
}

export default Navbar;