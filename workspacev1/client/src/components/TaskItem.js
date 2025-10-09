import React from 'react';
import { format } from 'date-fns';
import './TaskItem.css';

function TaskItem({ task, onEdit, onDelete }) {
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
        </div>
      </div>

      <p className="task-description">{task.description}</p>

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