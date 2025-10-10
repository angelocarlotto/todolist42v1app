import React, { useMemo } from 'react';
import { useApp } from '../context/AppContext';
import './TaskBoard.css';

function TaskBoard() {
  const { tasks, updateTask } = useApp();

  // Group tasks by status
  const columns = useMemo(() => ({
    todo: tasks.filter(t => t.status === 'ToDo'),
    doing: tasks.filter(t => t.status === 'InProgress'),
    done: tasks.filter(t => t.status === 'Done'),
  }), [tasks]);

  // Drag and drop handlers
  const onDragStart = (e, taskId) => {
    e.dataTransfer.setData('taskId', taskId);
  };

  const onDrop = async (e, newStatus) => {
    const taskId = e.dataTransfer.getData('taskId');
    const task = tasks.find(t => t.id === taskId);
    if (task && task.status !== newStatus) {
      await updateTask(taskId, { ...task, status: newStatus });
    }
  };

  const onDragOver = (e) => {
    e.preventDefault();
  };

  return (
    <div className="task-board">
      {['todo', 'doing', 'done'].map(col => (
        <div
          key={col}
          className={`task-column task-column-${col}`}
          onDrop={e => onDrop(e, col === 'todo' ? 'ToDo' : col === 'doing' ? 'InProgress' : 'Done')}
          onDragOver={onDragOver}
        >
          <h3>{col === 'todo' ? 'To Do' : col === 'doing' ? 'Doing' : 'Done'}</h3>
          {columns[col].map(task => (
            <div
              key={task.id}
              className="task-card"
              draggable
              onDragStart={e => onDragStart(e, task.id)}
            >
              <div className="task-title">{task.shortTitle}</div>
              <div className="task-desc">{task.description}</div>
              <div className="task-meta">Due: {new Date(task.dueDate).toLocaleDateString()}</div>
            </div>
          ))}
        </div>
      ))}
    </div>
  );
}

export default TaskBoard;
