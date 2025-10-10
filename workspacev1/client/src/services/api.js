import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5175';

class ApiService {
  constructor() {
    this.client = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Add auth token to requests
    this.client.interceptors.request.use((config) => {
      const token = localStorage.getItem('authToken');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });

    // Handle auth errors
    this.client.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response?.status === 401) {
          // Clear all auth-related data
          localStorage.removeItem('authToken');
          localStorage.removeItem('user');
          localStorage.removeItem('undefined');
          localStorage.removeItem('null');
          // Only redirect if we're not already on the login page
          if (window.location.pathname !== '/login') {
            window.location.href = '/login';
          }
        }
        return Promise.reject(error);
      }
    );
  }

  // Auth methods
  async login(username, password) {
    const response = await this.client.post('/api/auth/login', {
      username,
      password,
    });
    if (response.data.token && response.data.user) {
      localStorage.setItem('authToken', response.data.token);
      localStorage.setItem('user', JSON.stringify(response.data.user));
    }
    return response.data;
  }

  async register(username, password, tenantName) {
    const response = await this.client.post('/api/auth/register', {
      username,
      password,
      tenantName,
    });
    return response.data;
  }

  logout() {
    localStorage.removeItem('authToken');
    localStorage.removeItem('user');
    // Clear any potentially corrupted data
    localStorage.removeItem('undefined');
    localStorage.removeItem('null');
  }

  getCurrentUser() {
    const user = localStorage.getItem('user');
    if (!user || user === 'undefined' || user === 'null') {
      return null;
    }
    try {
      return JSON.parse(user);
    } catch (error) {
      console.error('Error parsing user data from localStorage:', error);
      // Clear corrupted data
      localStorage.removeItem('user');
      localStorage.removeItem('authToken');
      return null;
    }
  }

  // Task methods
  async getTasks() {
    const response = await this.client.get('/api/tasks');
    return response.data;
  }

  async getTask(id) {
    const response = await this.client.get(`/api/tasks/${id}`);
    return response.data;
  }

  async createTask(task) {
    const response = await this.client.post('/api/tasks', task);
    return response.data;
  }

  async updateTask(id, task) {
    const response = await this.client.put(`/api/tasks/${id}`, task);
    return response.data;
  }

  async deleteTask(id) {
    const response = await this.client.delete(`/api/tasks/${id}`);
    return response.data;
  }

  async assignUsers(taskId, userIds) {
    const response = await this.client.post(`/api/tasks/${taskId}/assign`, userIds);
    return response.data;
  }

  async unassignUsers(taskId, userIds) {
    const response = await this.client.post(`/api/tasks/${taskId}/unassign`, userIds);
    return response.data;
  }

  async uploadFiles(taskId, files) {
    const formData = new FormData();
    Array.from(files).forEach(file => {
      formData.append('files', file);
    });
    
    const response = await this.client.post(`/api/tasks/${taskId}/upload`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  }

  async deleteFile(taskId, filePath) {
    const response = await this.client.delete(`/api/tasks/${taskId}/files`, {
      data: { filePath }
    });
    return response.data;
  }

  // Public task methods
  async getPublicTask(publicShareId) {
    const response = await this.client.get(`/public/task/${publicShareId}`);
    return response.data;
  }

  async updatePublicTask(publicShareId, updates) {
    const response = await this.client.put(`/public/task/${publicShareId}`, updates);
    return response.data;
  }

  async shareTask(taskId) {
    const response = await this.client.post(`/api/share/${taskId}/share`);
    return response.data;
  }

  async shareTaskWithOptions(taskId, options) {
    const response = await this.client.post(`/api/share/${taskId}/share`, options);
    return response.data;
  }

  async revokeShare(taskId) {
    const response = await this.client.post(`/api/share/${taskId}/revoke-share`);
    return response.data;
  }

  // Comment methods
  async addComment(taskId, text) {
    const response = await this.client.post(`/api/tasks/${taskId}/comments`, { text });
    return response.data;
  }

  async getComments(taskId) {
    const response = await this.client.get(`/api/tasks/${taskId}/comments`);
    return response.data;
  }

  async deleteComment(taskId, commentId) {
    const response = await this.client.delete(`/api/tasks/${taskId}/comments/${commentId}`);
    return response.data;
  }

  // Tenant methods
  async getTenants() {
    const response = await this.client.get('/api/tenants');
    return response.data;
  }

  async createTenant(tenant) {
    const response = await this.client.post('/api/tenants', tenant);
    return response.data;
  }

  // Reminder methods
  async getReminders() {
    const response = await this.client.get('/api/tasks/reminders');
    return response.data;
  }

  async sendReminders() {
    const response = await this.client.post('/api/tasks/send-reminders');
    return response.data;
  }
}

export default new ApiService();