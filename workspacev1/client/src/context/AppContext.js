import React, { createContext, useContext, useReducer, useEffect, useCallback } from 'react';
import apiService from '../services/api';
import signalRService from '../services/signalr';

const AppContext = createContext();

const initialState = {
  user: null,
  tasks: [],
  tenants: [],
  reminders: [],
  loading: false,
  error: null,
  isAuthenticated: false,
  notifications: [],
};

function appReducer(state, action) {
  switch (action.type) {
    case 'SET_LOADING':
      return { ...state, loading: action.payload };
    case 'SET_ERROR':
      return { ...state, error: action.payload, loading: false };
    case 'SET_USER':
      return { ...state, user: action.payload, isAuthenticated: !!action.payload };
    case 'SET_TASKS':
      return { ...state, tasks: action.payload };
    case 'ADD_TASK':
      return { ...state, tasks: [...state.tasks, action.payload] };
    case 'UPDATE_TASK':
      const newTasks = state.tasks.map(task =>
        task.id === action.payload.id ? { ...action.payload } : task
      );
      return {
        ...state,
        tasks: newTasks,
      };
    case 'DELETE_TASK':
      return {
        ...state,
        tasks: state.tasks.filter(task => task.id !== action.payload),
      };
    case 'SET_TENANTS':
      return { ...state, tenants: action.payload };
    case 'SET_REMINDERS':
      return { ...state, reminders: action.payload };
    case 'ADD_NOTIFICATION':
      return {
        ...state,
        notifications: [...state.notifications, action.payload],
      };
    case 'REMOVE_NOTIFICATION':
      return {
        ...state,
        notifications: state.notifications.filter(n => n.id !== action.payload),
      };
    case 'LOGOUT':
      return { ...initialState };
    default:
      return state;
  }
}

export function AppProvider({ children }) {
  const [state, dispatch] = useReducer(appReducer, initialState);

  useEffect(() => {
    // Clean up any corrupted localStorage data first
    const cleanupLocalStorage = () => {
      const authToken = localStorage.getItem('authToken');
      const user = localStorage.getItem('user');
      
      // Remove any corrupted entries
      if (authToken === 'undefined' || authToken === 'null') {
        localStorage.removeItem('authToken');
      }
      if (user === 'undefined' || user === 'null') {
        localStorage.removeItem('user');
      }
    };
    
    cleanupLocalStorage();
    
    // Check for existing auth on app start
    const user = apiService.getCurrentUser();
    if (user) {
      dispatch({ type: 'SET_USER', payload: user });
      initializeSignalR();
      loadTasks().catch(error => {
        console.error('Failed to load tasks on initialization:', error);
        // If tasks fail to load, user is still considered authenticated
        // The error will be handled by the axios interceptor if it's a 401
      });
    }
  }, []);

  const initializeSignalR = async () => {
    try {
      await signalRService.start();
      
      // Set up SignalR event handlers
      signalRService.onTaskUpdated((task) => {
        dispatch({ type: 'UPDATE_TASK', payload: task });
      });

      signalRService.onTaskCreated((task) => {
        dispatch({ type: 'ADD_TASK', payload: task });
      });

      signalRService.onTaskDeleted((taskId) => {
        dispatch({ type: 'DELETE_TASK', payload: taskId });
      });

      signalRService.onReceiveReminder((reminder) => {
        addNotification(
          `Reminder: "${reminder.title}" is due ${new Date(reminder.dueDate).toLocaleDateString()}`,
          'warning'
        );
      });

      // Handle comment events - refresh the task to get updated comments and activity log
      signalRService.onCommentAdded(async (data) => {
        try {
          const updatedTask = await apiService.getTask(data.taskId);
          dispatch({ type: 'UPDATE_TASK', payload: updatedTask });
        } catch (error) {
          console.error('Failed to refresh task after comment added:', error);
        }
      });

      signalRService.onCommentDeleted(async (data) => {
        try {
          const updatedTask = await apiService.getTask(data.taskId);
          dispatch({ type: 'UPDATE_TASK', payload: updatedTask });
        } catch (error) {
          console.error('Failed to refresh task after comment deleted:', error);
        }
      });
    } catch (error) {
      console.error('Failed to initialize SignalR:', error);
    }
  };

  const addNotification = (message, type = 'info') => {
    const notification = {
      id: Date.now(),
      message,
      type,
      timestamp: new Date(),
    };
    dispatch({ type: 'ADD_NOTIFICATION', payload: notification });
    
    // Auto-remove after 5 seconds
    setTimeout(() => {
      dispatch({ type: 'REMOVE_NOTIFICATION', payload: notification.id });
    }, 5000);
  };

  const register = async (username, password) => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      dispatch({ type: 'SET_ERROR', payload: null });
      
      // Call the registration API
      const result = await apiService.register(username, password);
      
      // After successful registration, automatically log in
      const loginResult = await apiService.login(username, password);
      dispatch({ type: 'SET_USER', payload: loginResult.user });
      await initializeSignalR();
      
      // Load tasks but don't fail if tasks fail to load
      try {
        await loadTasks();
      } catch (taskError) {
        console.error('Failed to load tasks after registration:', taskError);
      }
      
      addNotification('Registration successful! Welcome!', 'success');
      return loginResult;
    } catch (error) {
      const errorMessage = error.response?.data?.message || 'Registration failed';
      dispatch({ type: 'SET_ERROR', payload: errorMessage });
      addNotification(errorMessage, 'error');
      throw error;
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  };

  const login = async (username, password) => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      dispatch({ type: 'SET_ERROR', payload: null });
      
      const result = await apiService.login(username, password);
      dispatch({ type: 'SET_USER', payload: result.user });
      await initializeSignalR();
      
      // Load tasks but don't fail the login if tasks fail to load
      try {
        await loadTasks();
      } catch (taskError) {
        console.error('Failed to load tasks after login:', taskError);
        // Don't throw - the login was successful, just task loading failed
      }
      
      addNotification('Login successful!', 'success');
      return result;
    } catch (error) {
      const errorMessage = error.response?.data?.message || 'Login failed';
      dispatch({ type: 'SET_ERROR', payload: errorMessage });
      addNotification(errorMessage, 'error');
      throw error;
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  };

  const logout = async () => {
    try {
      await signalRService.stop();
      apiService.logout();
      dispatch({ type: 'LOGOUT' });
    } catch (error) {
      console.error('Logout error:', error);
    }
  };

  const loadTasks = useCallback(async () => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      const tasks = await apiService.getTasks();
      dispatch({ type: 'SET_TASKS', payload: tasks });
    } catch (error) {
      dispatch({ type: 'SET_ERROR', payload: 'Failed to load tasks' });
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  }, []);

  const createTask = async (taskData) => {
    try {
      const task = await apiService.createTask(taskData);
      // Don't dispatch ADD_TASK here - the backend will broadcast TaskCreated via SignalR
      // and the onTaskCreated handler will add it to the state
      addNotification('Task created successfully', 'success');
      return task;
    } catch (error) {
      dispatch({ type: 'SET_ERROR', payload: 'Failed to create task' });
      throw error;
    }
  };

  const updateTask = async (id, taskData) => {
    try {
      await apiService.updateTask(id, taskData);
      // Don't dispatch UPDATE_TASK here - the backend will broadcast TaskUpdated via SignalR
      // and the onTaskUpdated handler will update it in the state
      addNotification('Task updated successfully', 'success');
    } catch (error) {
      dispatch({ type: 'SET_ERROR', payload: 'Failed to update task' });
      throw error;
    }
  };

  const deleteTask = async (id) => {
    try {
      await apiService.deleteTask(id);
      // Don't dispatch DELETE_TASK here - the backend will broadcast TaskDeleted via SignalR
      // and the onTaskDeleted handler will remove it from the state
      addNotification('Task deleted successfully', 'success');
    } catch (error) {
      dispatch({ type: 'SET_ERROR', payload: 'Failed to delete task' });
      throw error;
    }
  };

  const loadReminders = async () => {
    try {
      const reminders = await apiService.getReminders();
      dispatch({ type: 'SET_REMINDERS', payload: reminders });
    } catch (error) {
      dispatch({ type: 'SET_ERROR', payload: 'Failed to load reminders' });
    }
  };

  const value = {
    ...state,
    login,
    register,
    logout,
    loadTasks,
    createTask,
    updateTask,
    deleteTask,
    loadReminders,
    addNotification,
    dispatch,
  };

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>;
}

export function useApp() {
  const context = useContext(AppContext);
  if (!context) {
    throw new Error('useApp must be used within an AppProvider');
  }
  return context;
}