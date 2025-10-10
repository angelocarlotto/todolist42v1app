import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import apiService from '../services/api';
import signalRService from '../services/signalr';

export default function PublicTaskView() {
  const { publicShareId } = useParams();
  const [task, setTask] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [editing, setEditing] = useState(false);
  const [editData, setEditData] = useState({ description: '', status: '' });
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    let isMounted = true;
    async function fetchTask() {
      setLoading(true);
      try {
        const data = await apiService.getPublicTask(publicShareId);
        if (isMounted) setTask(data);
      } catch (err) {
        setError('Task not found or no longer shared.');
      } finally {
        setLoading(false);
      }
    }
    fetchTask();

    // Connect to SignalR and join public group (without authentication)
    signalRService.startPublic().then(() => {
      // Join the public share group to receive updates
      signalRService.joinGroup(publicShareId);
      
      signalRService.onTaskUpdated((updatedTask) => {
        setTask(updatedTask);
      });
      signalRService.onTaskDeleted(() => {
        setError('This task is no longer shared.');
        setTask(null);
      });
    }).catch(err => {
      console.error('Failed to connect to SignalR for public view:', err);
    });

    return () => {
      isMounted = false;
      signalRService.off('TaskUpdated');
      signalRService.off('TaskDeleted');
    };
  }, [publicShareId]);

  if (loading) return <div>Loading...</div>;
  if (error) return <div style={{color:'red'}}>{error}</div>;
  if (!task) return null;

  const handleEdit = () => {
    setEditData({ description: task.description || '', status: task.status || '' });
    setEditing(true);
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      const updated = await apiService.updatePublicTask(publicShareId, editData);
      setTask(updated);
      setEditing(false);
    } catch (err) {
      alert('Failed to update task: ' + (err.response?.data?.message || err.message));
    } finally {
      setSaving(false);
    }
  };

  const handleCancel = () => {
    setEditing(false);
  };

  return (
    <div className="public-task-view" style={{ padding: '20px', maxWidth: '800px', margin: '0 auto' }}>
      <div style={{ backgroundColor: '#fff3cd', padding: '10px', marginBottom: '20px', borderRadius: '5px' }}>
        ℹ️ You are viewing a publicly shared task. Changes you make will be visible to everyone.
      </div>
      
      <h2>{task.shortTitle}</h2>
      
      {!editing ? (
        <>
          <p><strong>Description:</strong> {task.description}</p>
          <p><strong>Status:</strong> {task.status}</p>
          <p><strong>Due:</strong> {new Date(task.dueDate).toLocaleDateString()}</p>
          <p><strong>Criticality:</strong> {task.criticality}</p>
          
          <button onClick={handleEdit} style={{ padding: '10px 20px', marginTop: '10px' }}>
            Edit Task
          </button>
        </>
      ) : (
        <div style={{ marginTop: '20px' }}>
          <div style={{ marginBottom: '15px' }}>
            <label style={{ display: 'block', marginBottom: '5px' }}>Description:</label>
            <textarea
              value={editData.description}
              onChange={(e) => setEditData({ ...editData, description: e.target.value })}
              style={{ width: '100%', minHeight: '100px', padding: '8px' }}
            />
          </div>
          
          <div style={{ marginBottom: '15px' }}>
            <label style={{ display: 'block', marginBottom: '5px' }}>Status:</label>
            <select
              value={editData.status}
              onChange={(e) => setEditData({ ...editData, status: e.target.value })}
              style={{ width: '100%', padding: '8px' }}
            >
              <option value="ToDo">To Do</option>
              <option value="InProgress">In Progress</option>
              <option value="Done">Done</option>
            </select>
          </div>
          
          <button onClick={handleSave} disabled={saving} style={{ padding: '10px 20px', marginRight: '10px' }}>
            {saving ? 'Saving...' : 'Save'}
          </button>
          <button onClick={handleCancel} disabled={saving} style={{ padding: '10px 20px' }}>
            Cancel
          </button>
        </div>
      )}
      
      {task.files && task.files.length > 0 && (
        <div>
          <h4>Files:</h4>
          <ul>
            {task.files.map((file, idx) => (
              <li key={idx}><a href={file} target="_blank" rel="noopener noreferrer">{file}</a></li>
            ))}
          </ul>
        </div>
      )}
      {task.tags && task.tags.length > 0 && (
        <div>
          <h4>Tags:</h4>
          <ul>
            {task.tags.map((tag, idx) => (
              <li key={idx}>{tag}</li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}
