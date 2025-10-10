// Simple test script to validate the login method
import apiService from './services/api.js';

async function testLogin() {
  console.log('Testing client login functionality...');
  
  try {
    // Test with valid credentials (assuming user exists from our previous API tests)
    const username = 'testuser';
    const password = '12345678';
    
    console.log(`Attempting to login with username: ${username}`);
    console.log(`API Base URL: ${apiService.client.defaults.baseURL}`);
    
    const result = await apiService.login(username, password);
    
    console.log('Login successful!');
    console.log('Response:', result);
    
    // Check if token was stored
    const storedToken = localStorage.getItem('authToken');
    const storedUser = localStorage.getItem('user');
    
    console.log('Stored token:', storedToken ? 'Present' : 'Missing');
    console.log('Stored user:', storedUser ? JSON.parse(storedUser) : 'Missing');
    
    // Test getCurrentUser method
    const currentUser = apiService.getCurrentUser();
    console.log('Current user from service:', currentUser);
    
    return true;
  } catch (error) {
    console.error('Login failed:', error);
    
    if (error.response) {
      console.error('Response status:', error.response.status);
      console.error('Response data:', error.response.data);
      console.error('Response headers:', error.response.headers);
    } else if (error.request) {
      console.error('No response received:', error.request);
    } else {
      console.error('Error message:', error.message);
    }
    
    return false;
  }
}

// Alternative test using fetch API directly
async function testLoginWithFetch() {
  console.log('\nTesting login with fetch API...');
  
  try {
    const response = await fetch('http://localhost:5175/api/auth/login', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        username: 'testuser',
        password: '12345678'
      })
    });
    
    console.log('Response status:', response.status);
    console.log('Response headers:', Object.fromEntries(response.headers.entries()));
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    const data = await response.json();
    console.log('Login successful with fetch!');
    console.log('Response data:', data);
    
    return true;
  } catch (error) {
    console.error('Fetch login failed:', error);
    return false;
  }
}

// Test function for registration
async function testRegistration() {
  console.log('\nTesting registration functionality...');
  
  try {
    const testUsername = 'newuser' + Date.now();
    const testPassword = '87654321';
    const testTenantName = 'TestTenant' + Date.now();
    
    console.log(`Registering user: ${testUsername}`);
    
    const result = await apiService.register(testUsername, testPassword, testTenantName);
    console.log('Registration successful!');
    console.log('Response:', result);
    
    return true;
  } catch (error) {
    console.error('Registration failed:', error);
    return false;
  }
}

// Main test runner
async function runAllTests() {
  console.log('=== Client Login Testing ===\n');
  
  const results = {
    loginTest: await testLogin(),
    fetchTest: await testLoginWithFetch(),
    registrationTest: await testRegistration()
  };
  
  console.log('\n=== Test Results ===');
  console.log('Login test:', results.loginTest ? 'PASSED' : 'FAILED');
  console.log('Fetch test:', results.fetchTest ? 'PASSED' : 'FAILED');
  console.log('Registration test:', results.registrationTest ? 'PASSED' : 'FAILED');
  
  return results;
}

// Export for use in browser console or testing
if (typeof window !== 'undefined') {
  window.testLogin = testLogin;
  window.testLoginWithFetch = testLoginWithFetch;
  window.testRegistration = testRegistration;
  window.runAllTests = runAllTests;
}

export { testLogin, testLoginWithFetch, testRegistration, runAllTests };