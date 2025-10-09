import React, { useEffect, useState } from 'react';
import { useApp } from '../context/AppContext';
import TaskItem from './TaskItem';
import TaskForm from './TaskForm';
import './TaskList.css';

function TaskList() {
  const { tasks, loading, loadTasks, createTask, updateTask, deleteTask } = useApp();
  const [showForm, setShowForm] = useState(false);
  const [editingTask, setEditingTask] = useState(null);
  const [filter, setFilter] = useState('all');
  const [sortBy, setSortBy] = useState('dueDate');

  useEffect(() => {
    loadTasks();
  }, [loadTasks]);

  const handleCreateTask = async (taskData) => {
    try {
      await createTask(taskData);
      setShowForm(false);
    } catch (error) {
      console.error('Create task error:', error);
    }
  };

  const handleUpdateTask = async (taskData) => {
    try {
      await updateTask(editingTask.id, taskData);
      setEditingTask(null);
    } catch (error) {
      console.error('Update task error:', error);
    }
  };

  const handleDeleteTask = async (taskId) => {
    if (window.confirm('Are you sure you want to delete this task?')) {
      try {
        await deleteTask(taskId);
      } catch (error) {
        console.error('Delete task error:', error);
      }
    }
  };

  const filteredTasks = tasks.filter(task => {
    switch (filter) {
      case 'todo':
        return task.status === 'ToDo';
      case 'inprogress':
        return task.status === 'InProgress';
      case 'done':
        return task.status === 'Done';
      default:
        return true;
    }
  });

  const sortedTasks = [...filteredTasks].sort((a, b) => {
    switch (sortBy) {
      case 'dueDate':
        return new Date(a.dueDate) - new Date(b.dueDate);
      case 'priority':
        const priorityOrder = { High: 3, Medium: 2, Low: 1 };
        return priorityOrder[b.criticality] - priorityOrder[a.criticality];
      case 'status':
        return a.status.localeCompare(b.status);
      case 'title':
        return a.shortTitle.localeCompare(b.shortTitle);
      default:
        return 0;
    }
  });

  if (loading && tasks.length === 0) {
    return <div className="loading">Loading tasks...</div>;
  }

  return (
    <div className="task-list-container">
      <div className="task-list-header">
        <h2>My Tasks</h2>
        <button
          className="btn btn-primary"
          onClick={() => setShowForm(true)}
        >
          + New Task
        </button>
      </div>

      <div className="task-controls">
        <div className="filter-controls">
          <label>Filter:</label>
          <select value={filter} onChange={(e) => setFilter(e.target.value)}>
            <option value="all">All Tasks</option>
            <option value="todo">To Do</option>
            <option value="inprogress">In Progress</option>
            <option value="done">Done</option>
          </select>
        </div>

        <div className="sort-controls">
          <label>Sort by:</label>
          <select value={sortBy} onChange={(e) => setSortBy(e.target.value)}>
            <option value="dueDate">Due Date</option>
            <option value="priority">Priority</option>
            <option value="status">Status</option>
            <option value="title">Title</option>
          </select>
        </div>
      </div>

      <div className="task-grid">
        {sortedTasks.map(task => (
          <TaskItem
            key={task.id}
            task={task}
            onEdit={setEditingTask}
            onDelete={handleDeleteTask}
          />
        ))}
      </div>

      {sortedTasks.length === 0 && (
        <div className="empty-state">
          <p>No tasks found. Create your first task to get started!</p>
        </div>
      )}

      {(showForm || editingTask) && (
        <TaskForm
          task={editingTask}
          onSubmit={editingTask ? handleUpdateTask : handleCreateTask}
          onCancel={() => {
            setShowForm(false);
            setEditingTask(null);
          }}
        />
      )}
    </div>
  );
}

export default TaskList;