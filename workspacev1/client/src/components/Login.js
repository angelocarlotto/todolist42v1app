import React, { useState } from 'react';
import { useApp } from '../context/AppContext';
import './Login.css';

function Login() {
  const [isLogin, setIsLogin] = useState(true);
  const [formData, setFormData] = useState({
    username: '',
    password: '',
    tenantName: '',
  });
  const { login, loading, error } = useApp();

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      if (isLogin) {
        await login(formData.username, formData.password);
      } else {
        // Handle registration - you'll need to implement this
        console.log('Registration not implemented yet');
      }
    } catch (error) {
      console.error('Auth error:', error);
    }
  };

  const handleInputChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <h2>{isLogin ? 'Login' : 'Register'}</h2>
        
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
              required
              disabled={loading}
            />
          </div>
          
          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              type="password"
              id="password"
              name="password"
              value={formData.password}
              onChange={handleInputChange}
              required
              disabled={loading}
            />
          </div>
          
          {!isLogin && (
            <div className="form-group">
              <label htmlFor="tenantName">Organization Name</label>
              <input
                type="text"
                id="tenantName"
                name="tenantName"
                value={formData.tenantName}
                onChange={handleInputChange}
                required
                disabled={loading}
              />
            </div>
          )}
          
          <button 
            type="submit" 
            className="btn btn-primary"
            disabled={loading}
          >
            {loading ? 'Loading...' : (isLogin ? 'Login' : 'Register')}
          </button>
        </form>
        
        <div className="auth-toggle">
          <button
            type="button"
            className="btn btn-link"
            onClick={() => setIsLogin(!isLogin)}
            disabled={loading}
          >
            {isLogin ? 'Need to register?' : 'Already have an account?'}
          </button>
        </div>
      </div>
    </div>
  );
}

export default Login;