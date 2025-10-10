import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { format } from 'date-fns';
import apiService from '../services/api';
import signalRService from '../services/signalr';

export default function PublicTaskView() {
  const { publicShareId } = useParams();
  const [task, setTask] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [errorType, setErrorType] = useState(null); // 'expired', 'maxViews', 'notFound', 'editNotAllowed'
  const [editing, setEditing] = useState(false);
  const [editData, setEditData] = useState({ description: '', status: '' });
  const [saving, setSaving] = useState(false);
  const [timeRemaining, setTimeRemaining] = useState('');

  // Countdown timer for expiration
  useEffect(() => {
    if (!task?.shareExpiresAt) return;

    const updateCountdown = () => {
      const now = new Date();
      const expiresAt = new Date(task.shareExpiresAt);
      const diff = expiresAt - now;

      if (diff <= 0) {
        setTimeRemaining('Expired');
        setError('This share link has expired.');
        setErrorType('expired');
        return;
      }

      const days = Math.floor(diff / (1000 * 60 * 60 * 24));
      const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
      const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
      const seconds = Math.floor((diff % (1000 * 60)) / 1000);

      let countdown = '';
      if (days > 0) countdown += `${days}d `;
      if (days > 0 || hours > 0) countdown += `${hours}h `;
      if (days > 0 || hours > 0 || minutes > 0) countdown += `${minutes}m `;
      countdown += `${seconds}s`;

      setTimeRemaining(countdown);
    };

    updateCountdown();
    const interval = setInterval(updateCountdown, 1000);

    return () => clearInterval(interval);
  }, [task?.shareExpiresAt]);

  useEffect(() => {
    let isMounted = true;
    async function fetchTask() {
      setLoading(true);
      setError(null);
      setErrorType(null);
      try {
        const data = await apiService.getPublicTask(publicShareId);
        if (isMounted) setTask(data);
      } catch (err) {
        if (err.response?.status === 410) {
          setErrorType('expired');
          setError('This share link has expired.');
        } else if (err.response?.status === 403) {
          setErrorType('maxViews');
          setError('This share link has reached its maximum number of views.');
        } else {
          setErrorType('notFound');
          setError('Task not found or no longer shared.');
        }
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
        setErrorType('notFound');
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

  if (loading) return <div style={{ padding: '20px', textAlign: 'center' }}>Loading...</div>;
  
  if (error) {
    return (
      <div style={{ padding: '20px', maxWidth: '600px', margin: '0 auto' }}>
        <div style={{ 
          backgroundColor: errorType === 'expired' || errorType === 'maxViews' ? '#fff3cd' : '#f8d7da', 
          padding: '20px', 
          borderRadius: '8px',
          textAlign: 'center',
          color: errorType === 'expired' || errorType === 'maxViews' ? '#856404' : '#721c24'
        }}>
          <h2>{errorType === 'expired' ? '⏰ Link Expired' : errorType === 'maxViews' ? '👁️ View Limit Reached' : '❌ Not Found'}</h2>
          <p style={{ fontSize: '1.1rem', marginTop: '10px' }}>{error}</p>
          {(errorType === 'expired' || errorType === 'maxViews') && (
            <p style={{ marginTop: '15px', fontSize: '0.9rem' }}>
              Please contact the task owner for a new share link.
            </p>
          )}
        </div>
      </div>
    );
  }
  
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
      if (err.response?.status === 403) {
        setErrorType('editNotAllowed');
        setError('Editing is not allowed for this shared task.');
        setEditing(false);
      } else if (err.response?.status === 410) {
        setErrorType('expired');
        setError('This share link has expired.');
        setEditing(false);
      } else {
        alert('Failed to update task: ' + (err.response?.data?.message || err.message));
      }
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

      {/* Expiration and limits info */}
      {(task.shareExpiresAt || task.shareMaxViews) && (
        <div style={{ 
          backgroundColor: '#e7f3ff', 
          padding: '10px', 
          marginBottom: '20px', 
          borderRadius: '5px',
          fontSize: '0.9rem'
        }}>
          {task.shareExpiresAt && (
            <div style={{ marginBottom: '5px' }}>
              <div>⏰ Expires: {format(new Date(task.shareExpiresAt), 'MMM dd, yyyy HH:mm')}</div>
              {timeRemaining && (
                <div style={{ 
                  fontWeight: 'bold', 
                  color: timeRemaining === 'Expired' ? '#dc3545' : '#0066cc',
                  marginTop: '3px',
                  fontSize: '1rem'
                }}>
                  {timeRemaining === 'Expired' ? '❌ Expired' : `⏱️ Time remaining: ${timeRemaining}`}
                </div>
              )}
            </div>
          )}
          {task.shareMaxViews && (
            <div style={{ marginTop: task.shareExpiresAt ? '8px' : '0' }}>
              👁️ View limit: {task.shareViewCount || 0} / {task.shareMaxViews}
            </div>
          )}
          {!task.shareAllowEdit && (
            <div style={{ marginTop: '8px' }}>🔒 Read-only access (editing disabled)</div>
          )}
        </div>
      )}
      
      <h2>{task.shortTitle}</h2>
      
      {!editing ? (
        <>
          <p><strong>Description:</strong> {task.description}</p>
          <p><strong>Status:</strong> {task.status}</p>
          <p><strong>Due:</strong> {format(new Date(task.dueDate), 'MMM dd, yyyy')}</p>
          <p><strong>Criticality:</strong> {task.criticality}</p>
          
          {task.shareAllowEdit !== false && (
            <button onClick={handleEdit} style={{ padding: '10px 20px', marginTop: '10px' }}>
              Edit Task
            </button>
          )}
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
