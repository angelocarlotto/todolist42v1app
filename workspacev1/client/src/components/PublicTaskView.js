import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import apiService from '../services/api';
import signalRService from '../services/signalr';

export default function PublicTaskView() {
  const { publicShareId } = useParams();
  const [task, setTask] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

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

  return (
    <div className="public-task-view">
      <h2>{task.shortTitle}</h2>
      <p>{task.description}</p>
      <p>Status: {task.status}</p>
      <p>Due: {new Date(task.dueDate).toLocaleDateString()}</p>
      <p>Criticality: {task.criticality}</p>
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
