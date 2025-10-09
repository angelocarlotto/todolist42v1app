import React from 'react';
import { AppProvider, useApp } from './context/AppContext';
import Login from './components/Login';
import Navbar from './components/Navbar';
import TaskList from './components/TaskList';
import Notifications from './components/Notifications';
import './App.css';

function AppContent() {
  const { isAuthenticated } = useApp();

  if (!isAuthenticated) {
    return <Login />;
  }

  return (
    <div className="app-container">
      <Navbar />
      <main className="main-content">
        <TaskList />
      </main>
      <Notifications />
    </div>
  );
}

function App() {
  return (
    <AppProvider>
      <AppContent />
    </AppProvider>
  );
}

export default App;
