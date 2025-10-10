import React, { useState, useEffect } from 'react';
import './ShareOptionsDialog.css';

function ShareOptionsDialog({ isOpen, onClose, onShare, initialValues }) {
  const [expiresInHours, setExpiresInHours] = useState('');
  const [expiresInDays, setExpiresInDays] = useState('');
  const [maxViews, setMaxViews] = useState('');
  const [allowEdit, setAllowEdit] = useState(false);

  // Populate form with initial values when dialog opens
  useEffect(() => {
    if (isOpen && initialValues) {
      setExpiresInHours(initialValues.expiresInHours || '');
      setExpiresInDays(initialValues.expiresInDays || '');
      setMaxViews(initialValues.maxViews || '');
      setAllowEdit(initialValues.allowEdit || false);
    }
  }, [isOpen, initialValues]);

  const handleShare = () => {
    const options = {
      expiresInHours: expiresInHours ? parseInt(expiresInHours) : null,
      expiresInDays: expiresInDays ? parseInt(expiresInDays) : null,
      maxViews: maxViews ? parseInt(maxViews) : null,
      allowEdit
    };
    onShare(options);
    handleClose();
  };

  const handleClose = () => {
    setExpiresInHours('');
    setExpiresInDays('');
    setMaxViews('');
    setAllowEdit(false);
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div className="share-dialog-overlay" onClick={handleClose}>
      <div className="share-dialog" onClick={(e) => e.stopPropagation()}>
        <div className="share-dialog-header">
          <h3>{initialValues ? 'Update Share Options' : 'Share Options'}</h3>
          <button className="close-btn" onClick={handleClose}>×</button>
        </div>
        
        <div className="share-dialog-body">
          <div className="form-group">
            <label>Expiration</label>
            <div className="expiration-inputs">
              <div className="input-with-label">
                <input
                  type="number"
                  min="0"
                  value={expiresInHours}
                  onChange={(e) => setExpiresInHours(e.target.value)}
                  placeholder="Hours"
                />
                <span className="input-suffix">hours</span>
              </div>
              <div className="input-with-label">
                <input
                  type="number"
                  min="0"
                  value={expiresInDays}
                  onChange={(e) => setExpiresInDays(e.target.value)}
                  placeholder="Days"
                />
                <span className="input-suffix">days</span>
              </div>
            </div>
            <small className="form-text">Leave both empty for no expiration</small>
          </div>

          <div className="form-group">
            <label htmlFor="maxViews">Maximum Views</label>
            <input
              id="maxViews"
              type="number"
              min="1"
              value={maxViews}
              onChange={(e) => setMaxViews(e.target.value)}
              placeholder="Unlimited"
            />
            <small className="form-text">Leave empty for unlimited views</small>
          </div>

          <div className="form-group checkbox-group">
            <label>
              <input
                type="checkbox"
                checked={allowEdit}
                onChange={(e) => setAllowEdit(e.target.checked)}
              />
              <span>Allow public editing</span>
            </label>
            <small className="form-text">Viewers can edit the task</small>
          </div>
        </div>

        <div className="share-dialog-footer">
          <button className="btn btn-secondary" onClick={handleClose}>
            Cancel
          </button>
          <button className="btn btn-primary" onClick={handleShare}>
            {initialValues ? 'Update Link' : 'Generate Link'}
          </button>
        </div>
      </div>
    </div>
  );
}

export default ShareOptionsDialog;
