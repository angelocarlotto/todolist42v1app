import React from 'react';
import { useApp } from '../context/AppContext';
import './Notifications.css';

function Notifications() {
  const { notifications, dispatch } = useApp();

  const removeNotification = (id) => {
    dispatch({ type: 'REMOVE_NOTIFICATION', payload: id });
  };

  if (notifications.length === 0) {
    return null;
  }

  return (
    <div className="notifications-container">
      {notifications.map((notification) => (
        <div
          key={notification.id}
          className={`notification notification-${notification.type}`}
        >
          <div className="notification-content">
            <span>{notification.message}</span>
            <button
              className="notification-close"
              onClick={() => removeNotification(notification.id)}
            >
              ×
            </button>
          </div>
        </div>
      ))}
    </div>
  );
}

export default Notifications;