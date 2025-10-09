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
      
      <div className="navbar-content">
        {notifications.length > 0 && (
          <div className="notifications">
            <span className="notification-badge">{notifications.length}</span>
            🔔
          </div>
        )}
        
        <div className="user-info">
          <span>Welcome, {user?.username}</span>
          <button className="btn btn-link" onClick={handleLogout}>
            Logout
          </button>
        </div>
      </div>
    </nav>
  );
}

export default Navbar;