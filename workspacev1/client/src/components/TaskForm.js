import React, { useState, useEffect } from 'react';
import './TaskForm.css';

function TaskForm({ task, onSubmit, onCancel }) {
  const [formData, setFormData] = useState({
    shortTitle: '',
    description: '',
    dueDate: '',
    status: 'ToDo',
    criticality: 'Medium',
    tags: '',
  });

  useEffect(() => {
    if (task) {
      setFormData({
        shortTitle: task.shortTitle || '',
        description: task.description || '',
        dueDate: task.dueDate ? task.dueDate.split('T')[0] : '',
        status: task.status || 'ToDo',
        criticality: task.criticality || 'Medium',
        tags: task.tags ? task.tags.join(', ') : '',
      });
    }
  }, [task]);

  const handleSubmit = (e) => {
    e.preventDefault();
    
    const taskData = {
      ...formData,
      dueDate: new Date(formData.dueDate).toISOString(),
      tags: formData.tags.split(',').map(tag => tag.trim()).filter(tag => tag),
    };

    onSubmit(taskData);
  };

  const handleInputChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <div className="modal-header">
          <h3>{task ? 'Edit Task' : 'Create New Task'}</h3>
          <button className="btn-close" onClick={onCancel}>
            ×
          </button>
        </div>

        <form onSubmit={handleSubmit} className="task-form">
          <div className="form-group">
            <label htmlFor="shortTitle">Title *</label>
            <input
              type="text"
              id="shortTitle"
              name="shortTitle"
              value={formData.shortTitle}
              onChange={handleInputChange}
              required
              maxLength={100}
            />
          </div>

          <div className="form-group">
            <label htmlFor="description">Description *</label>
            <textarea
              id="description"
              name="description"
              value={formData.description}
              onChange={handleInputChange}
              required
              rows={4}
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="dueDate">Due Date *</label>
              <input
                type="date"
                id="dueDate"
                name="dueDate"
                value={formData.dueDate}
                onChange={handleInputChange}
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="status">Status</label>
              <select
                id="status"
                name="status"
                value={formData.status}
                onChange={handleInputChange}
              >
                <option value="ToDo">To Do</option>
                <option value="InProgress">In Progress</option>
                <option value="Done">Done</option>
              </select>
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="criticality">Priority</label>
              <select
                id="criticality"
                name="criticality"
                value={formData.criticality}
                onChange={handleInputChange}
              >
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="tags">Tags (comma-separated)</label>
              <input
                type="text"
                id="tags"
                name="tags"
                value={formData.tags}
                onChange={handleInputChange}
                placeholder="work, urgent, project"
              />
            </div>
          </div>

          <div className="form-actions">
            <button type="button" className="btn btn-secondary" onClick={onCancel}>
              Cancel
            </button>
            <button type="submit" className="btn btn-primary">
              {task ? 'Update Task' : 'Create Task'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default TaskForm;