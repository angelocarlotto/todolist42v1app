import React, { useState, useEffect } from 'react';
import { format } from 'date-fns';
import apiService from '../services/api';
import './CommentsSection.css';

function CommentsSection({ taskId, initialComments = [] }) {
  const [comments, setComments] = useState(initialComments);
  const [newComment, setNewComment] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    setComments(initialComments);
  }, [initialComments]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!newComment.trim()) return;

    setSubmitting(true);
    setError(null);

    try {
      await apiService.addComment(taskId, newComment.trim());
      // Comment will be added via SignalR event automatically - no need to add locally
      setNewComment('');
    } catch (err) {
      setError('Failed to add comment. Please try again.');
      console.error('Error adding comment:', err);
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (commentId) => {
    try {
      await apiService.deleteComment(taskId, commentId);
      // Comment will be removed via SignalR event
    } catch (err) {
      setError('Failed to delete comment.');
      console.error('Error deleting comment:', err);
    }
  };

  return (
    <div className="comments-section">
      <h4>Comments ({comments.length})</h4>
      
      {error && <div className="comment-error">{error}</div>}

      <div className="comments-list">
        {comments.length === 0 ? (
          <p className="no-comments">No comments yet. Be the first to comment!</p>
        ) : (
          comments.map(comment => (
            <div key={comment.id} className="comment-item">
              <div className="comment-header">
                <span className="comment-author">{comment.username}</span>
                <span className="comment-date">
                  {format(new Date(comment.createdAt), 'MMM dd, yyyy HH:mm')}
                </span>
              </div>
              <div className="comment-text">{comment.text}</div>
              <button 
                className="comment-delete-btn"
                onClick={() => handleDelete(comment.id)}
                title="Delete comment"
              >
                🗑️
              </button>
            </div>
          ))
        )}
      </div>

      <form className="comment-form" onSubmit={handleSubmit}>
        <textarea
          value={newComment}
          onChange={(e) => setNewComment(e.target.value)}
          placeholder="Write a comment..."
          rows="3"
          disabled={submitting}
        />
        <button type="submit" disabled={submitting || !newComment.trim()}>
          {submitting ? 'Posting...' : 'Post Comment'}
        </button>
      </form>
    </div>
  );
}

export default CommentsSection;
