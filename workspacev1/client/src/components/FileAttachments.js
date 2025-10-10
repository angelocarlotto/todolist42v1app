import React, { useState } from 'react';
import apiService from '../services/api';
import './FileAttachments.css';

function FileAttachments({ taskId, files = [] }) {
  const [uploading, setUploading] = useState(false);
  const [deleting, setDeleting] = useState(null);
  const [error, setError] = useState(null);

  const handleFileSelect = async (e) => {
    const selectedFiles = e.target.files;
    if (!selectedFiles || selectedFiles.length === 0) return;

    setUploading(true);
    setError(null);

    try {
      await apiService.uploadFiles(taskId, selectedFiles);
      // The backend will broadcast the update via SignalR
      e.target.value = ''; // Reset file input
    } catch (err) {
      console.error('Failed to upload files:', err);
      setError('Failed to upload files. Please try again.');
    } finally {
      setUploading(false);
    }
  };

  const handleDeleteFile = async (filePath) => {
    if (!window.confirm('Are you sure you want to delete this file?')) {
      return;
    }

    setDeleting(filePath);
    setError(null);

    try {
      await apiService.deleteFile(taskId, filePath);
      // The backend will broadcast the update via SignalR
    } catch (err) {
      console.error('Failed to delete file:', err);
      setError('Failed to delete file. Please try again.');
    } finally {
      setDeleting(null);
    }
  };

  const getFileName = (filePath) => {
    // Extract original filename from path like "/uploads/guid_originalname.ext"
    const parts = filePath.split('_');
    if (parts.length > 1) {
      return parts.slice(1).join('_'); // Join back in case filename had underscores
    }
    return filePath.split('/').pop();
  };

  const getFileIcon = (fileName) => {
    const ext = fileName.split('.').pop().toLowerCase();
    const iconMap = {
      pdf: '📄',
      doc: '📝',
      docx: '📝',
      xls: '📊',
      xlsx: '📊',
      ppt: '📊',
      pptx: '📊',
      txt: '📃',
      jpg: '🖼️',
      jpeg: '🖼️',
      png: '🖼️',
      gif: '🖼️',
      zip: '🗜️',
      rar: '🗜️',
    };
    return iconMap[ext] || '📎';
  };

  const formatFileSize = (bytes) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  };

  return (
    <div className="file-attachments">
      <div className="attachments-header">
        <h4>📎 Attachments ({files.length})</h4>
        <label className="upload-btn">
          <input
            type="file"
            multiple
            onChange={handleFileSelect}
            disabled={uploading}
            style={{ display: 'none' }}
          />
          {uploading ? 'Uploading...' : '+ Add Files'}
        </label>
      </div>

      {error && <div className="error-message">{error}</div>}

      {files.length > 0 && (
        <div className="file-list">
          {files.map((file, index) => {
            const fileName = getFileName(file);
            const fileIcon = getFileIcon(fileName);
            
            return (
              <div key={index} className="file-item">
                <span className="file-icon">{fileIcon}</span>
                <div className="file-info">
                  <a
                    href={`http://localhost:5175${file}`}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="file-name"
                  >
                    {fileName}
                  </a>
                </div>
                <a
                  href={`http://localhost:5175${file}`}
                  download
                  className="download-btn"
                  title="Download"
                >
                  ⬇️
                </a>
                <button
                  onClick={() => handleDeleteFile(file)}
                  className="delete-file-btn"
                  disabled={deleting === file}
                  title="Delete file"
                >
                  {deleting === file ? '⏳' : '🗑️'}
                </button>
              </div>
            );
          })}
        </div>
      )}

      {files.length === 0 && !uploading && (
        <div className="no-files">
          No files attached yet. Click "Add Files" to upload.
        </div>
      )}
    </div>
  );
}

export default FileAttachments;
