import React, { useState } from 'react';
import { useApp } from '../context/AppContext';
import './Login.css';

function Login() {
  const [isLogin, setIsLogin] = useState(true);
  const [formData, setFormData] = useState({
    username: '',
    password: '',
  });
  const { login, register, loading, error } = useApp();

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    // Basic validation
    if (!formData.username || !formData.password) {
      return;
    }

    // Password validation for registration
    if (!isLogin) {
      if (formData.password.length !== 8) {
        alert('Password must be exactly 8 characters');
        return;
      }
      if (!/^\d+$/.test(formData.password)) {
        alert('Password must contain only digits (0-9)');
        return;
      }
    }

    try {
      if (isLogin) {
        await login(formData.username, formData.password);
      } else {
        await register(formData.username, formData.password);
      }
    } catch (error) {
      console.error('Auth error:', error);
      // Error is already handled in AppContext with notifications
    }
  };

  const handleInputChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const toggleMode = () => {
    setIsLogin(!isLogin);
    // Clear form data when switching modes
    setFormData({
      username: '',
      password: '',
    });
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <h2>{isLogin ? 'Login to TaskFlow' : 'Create Account'}</h2>
        
        {error && <div className="error-message">{error}</div>}
        
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="username">Username</label>
            <input
              type="text"
              id="username"
              name="username"
              value={formData.username}
              onChange={handleInputChange}
              placeholder="Enter your username"
              required
              disabled={loading}
              autoComplete="username"
            />
          </div>
          
          <div className="form-group">
            <label htmlFor="password">
              Password
              {!isLogin && <span className="password-hint"> (8 digits only)</span>}
            </label>
            <input
              type="password"
              id="password"
              name="password"
              value={formData.password}
              onChange={handleInputChange}
              placeholder={isLogin ? "Enter your password" : "Enter 8 digits (e.g., 12345678)"}
              required
              disabled={loading}
              autoComplete={isLogin ? "current-password" : "new-password"}
              maxLength={isLogin ? undefined : 8}
              pattern={isLogin ? undefined : "\\d{8}"}
              title={isLogin ? undefined : "Password must be exactly 8 digits"}
            />
            {!isLogin && (
              <small className="form-text">
                Password must be exactly 8 digits (0-9)
              </small>
            )}
          </div>
          
          <button 
            type="submit" 
            className="btn btn-primary"
            disabled={loading}
          >
            {loading ? 'Please wait...' : (isLogin ? 'Login' : 'Create Account')}
          </button>
        </form>
        
        <div className="auth-toggle">
          <button
            type="button"
            className="btn btn-link"
            onClick={toggleMode}
            disabled={loading}
          >
            {isLogin ? "Don't have an account? Register" : 'Already have an account? Login'}
          </button>
        </div>

        {!isLogin && (
          <div className="info-message">
            <p>✨ A new organization will be created automatically for you!</p>
          </div>
        )}
      </div>
    </div>
  );
}

export default Login;