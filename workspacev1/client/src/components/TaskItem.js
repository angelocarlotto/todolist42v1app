import React, { useState } from 'react';
import { format } from 'date-fns';
import apiService from '../services/api';
import ShareOptionsDialog from './ShareOptionsDialog';
import './TaskItem.css';

function TaskItem({ task, onEdit, onDelete }) {
  const [shareLink, setShareLink] = useState(null);
  const [shareLoading, setShareLoading] = useState(false);
  const [shareError, setShareError] = useState(null);
  const [showShareDialog, setShowShareDialog] = useState(false);
  const [initialShareValues, setInitialShareValues] = useState(null);

  const handleShareClick = () => {
    // Calculate existing share settings if task is already shared
    if (task.publicShareId) {
      const values = {
        maxViews: task.shareMaxViews || '',
        allowEdit: task.shareAllowEdit || false,
        expiresInHours: '',
        expiresInDays: ''
      };

      // Calculate remaining time if there's an expiration
      if (task.shareExpiresAt) {
        const now = new Date();
        const expiresAt = new Date(task.shareExpiresAt);
        const diffMs = expiresAt - now;
        
        if (diffMs > 0) {
          const totalHours = Math.ceil(diffMs / (1000 * 60 * 60));
          const days = Math.floor(totalHours / 24);
          const hours = totalHours % 24;
          
          values.expiresInDays = days > 0 ? days : '';
          values.expiresInHours = hours > 0 ? hours : '';
        }
      }

      setInitialShareValues(values);
    } else {
      setInitialShareValues(null);
    }
    
    setShowShareDialog(true);
  };

  const handleShare = async (options) => {
    setShareLoading(true);
    setShareError(null);
    try {
      let publicShareId = task.publicShareId;
      if (!publicShareId) {
        const res = await apiService.shareTaskWithOptions(task.id, options);
        publicShareId = res.publicShareId;
      } else {
        // Re-share with new options
        await apiService.revokeShare(task.id);
        const res = await apiService.shareTaskWithOptions(task.id, options);
        publicShareId = res.publicShareId;
      }
      const url = `${window.location.origin}/public/task/${publicShareId}`;
      setShareLink(url);
      await navigator.clipboard.writeText(url);
    } catch (err) {
      setShareError('Failed to generate/copy share link.');
    } finally {
      setShareLoading(false);
    }
  };
  const getPriorityClass = (criticality) => {
    switch (criticality) {
      case 'High':
        return 'priority-high';
      case 'Medium':
        return 'priority-medium';
      case 'Low':
        return 'priority-low';
      default:
        return '';
    }
  };

  const getStatusClass = (status) => {
    switch (status) {
      case 'ToDo':
        return 'status-todo';
      case 'InProgress':
        return 'status-inprogress';
      case 'Done':
        return 'status-done';
      default:
        return '';
    }
  };

  const isOverdue = new Date(task.dueDate) < new Date() && task.status !== 'Done';

  return (
    <div className={`task-item ${isOverdue ? 'overdue' : ''}`}>
      <ShareOptionsDialog
        isOpen={showShareDialog}
        onClose={() => setShowShareDialog(false)}
        onShare={handleShare}
        initialValues={initialShareValues}
      />
      
      <div className="task-header">
        <h3 className="task-title">{task.shortTitle}</h3>
        <div className="task-actions">
          <button
            className="btn btn-sm btn-secondary"
            onClick={() => onEdit(task)}
            title="Edit task"
          >
            ✏️
          </button>
          <button
            className="btn btn-sm btn-danger"
            onClick={() => onDelete(task.id)}
            title="Delete task"
          >
            🗑️
          </button>
          <button
            className="btn btn-sm btn-info"
            onClick={handleShareClick}
            disabled={shareLoading}
            title="Share public link"
          >
            🔗
          </button>
        </div>
      </div>

      <p className="task-description">{task.description}</p>
      {shareLink && (
        <div className="share-link-info">
          <span>Public link copied!</span>
          <a href={shareLink} target="_blank" rel="noopener noreferrer">Open</a>
        </div>
      )}
      {shareError && <div className="share-link-error" style={{color:'red'}}>{shareError}</div>}

      <div className="task-meta">
        <span className={`task-status ${getStatusClass(task.status)}`}>
          {task.status}
        </span>
        <span className={`task-priority ${getPriorityClass(task.criticality)}`}>
          {task.criticality}
        </span>
      </div>

      <div className="task-dates">
        <div className="due-date">
          <strong>Due:</strong> {format(new Date(task.dueDate), 'MMM dd, yyyy')}
          {isOverdue && <span className="overdue-indicator">⚠️ Overdue</span>}
        </div>
        {task.createdAt && (
          <div className="created-date">
            <small>Created: {format(new Date(task.createdAt), 'MMM dd, yyyy')}</small>
          </div>
        )}
      </div>

      {task.tags && task.tags.length > 0 && (
        <div className="task-tags">
          {task.tags.map((tag, index) => (
            <span key={index} className="tag">
              {tag}
            </span>
          ))}
        </div>
      )}

      {task.assignedUsers && task.assignedUsers.length > 0 && (
        <div className="assigned-users">
          <strong>Assigned to:</strong> {task.assignedUsers.length} user(s)
        </div>
      )}

      {task.files && task.files.length > 0 && (
        <div className="task-files">
          <strong>Files:</strong> {task.files.length} attachment(s)
        </div>
      )}
    </div>
  );
}

export default TaskItem;