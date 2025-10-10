import React from 'react';
import { format } from 'date-fns';
import './ActivityLog.css';

function ActivityLog({ activities = [] }) {
  const getActivityIcon = (activityType) => {
    switch (activityType) {
      case 'Created':
        return '✨';
      case 'Updated':
        return '✏️';
      case 'StatusChanged':
        return '🔄';
      case 'Commented':
        return '💬';
      case 'CommentDeleted':
        return '🗑️';
      case 'Deleted':
        return '❌';
      default:
        return '📌';
    }
  };

  const getActivityClass = (activityType) => {
    switch (activityType) {
      case 'Created':
        return 'activity-created';
      case 'StatusChanged':
        return 'activity-status-changed';
      case 'Commented':
        return 'activity-commented';
      case 'Deleted':
      case 'CommentDeleted':
        return 'activity-deleted';
      default:
        return 'activity-default';
    }
  };

  // Sort activities by timestamp, most recent first
  const sortedActivities = [...activities].sort((a, b) => 
    new Date(b.timestamp) - new Date(a.timestamp)
  );

  return (
    <div className="activity-log">
      <h4>Activity Log ({activities.length})</h4>
      
      {activities.length === 0 ? (
        <p className="no-activity">No activity yet.</p>
      ) : (
        <div className="activity-timeline">
          {sortedActivities.map(activity => (
            <div key={activity.id} className={`activity-item ${getActivityClass(activity.activityType)}`}>
              <div className="activity-icon">
                {getActivityIcon(activity.activityType)}
              </div>
              <div className="activity-content">
                <div className="activity-header">
                  <span className="activity-user">{activity.username}</span>
                  <span className="activity-time">
                    {format(new Date(activity.timestamp), 'MMM dd, HH:mm')}
                  </span>
                </div>
                <div className="activity-description">
                  {activity.description}
                  {activity.oldValue && activity.newValue && (
                    <span className="activity-change">
                      {' '}(<strong>{activity.oldValue}</strong> → <strong>{activity.newValue}</strong>)
                    </span>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default ActivityLog;
