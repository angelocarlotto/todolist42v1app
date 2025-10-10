import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

class SignalRService {
  constructor() {
    this.connection = null;
  }

  async start() {
    try {
      const token = localStorage.getItem('authToken');
      this.connection = new HubConnectionBuilder()
        .withUrl('http://localhost:5175/hub/collaboration', {
          accessTokenFactory: () => token
        })
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();

      await this.connection.start();
      console.log('SignalR Connected');

      // Join user's group and tenant group for notifications (only if authenticated)
      const user = JSON.parse(localStorage.getItem('user') || '{}');
      if (user.id) {
        await this.connection.invoke('JoinGroup', user.id);
      }
      if (user.tenantId) {
        await this.connection.invoke('JoinGroup', user.tenantId);
      }

      return this.connection;
    } catch (error) {
      console.error('SignalR Connection Error:', error);
      throw error;
    }
  }

  async startPublic() {
    try {
      // Public connection without authentication
      this.connection = new HubConnectionBuilder()
        .withUrl('http://localhost:5175/hub/collaboration')
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();

      await this.connection.start();
      console.log('SignalR Connected (Public)');

      return this.connection;
    } catch (error) {
      console.error('SignalR Connection Error:', error);
      throw error;
    }
  }

  async joinGroup(groupName) {
    if (this.connection && this.connection.state === 'Connected') {
      try {
        await this.connection.invoke('JoinGroup', groupName);
        console.log(`Joined group: ${groupName}`);
      } catch (error) {
        console.error(`Error joining group ${groupName}:`, error);
      }
    }
  }

  async stop() {
    if (this.connection) {
      await this.connection.stop();
      console.log('SignalR Disconnected');
    }
  }

  onTaskUpdated(callback) {
    if (this.connection) {
      this.connection.on('TaskUpdated', callback);
    }
  }

  onTaskCreated(callback) {
    if (this.connection) {
      this.connection.on('TaskCreated', callback);
    }
  }

  onTaskDeleted(callback) {
    if (this.connection) {
      this.connection.on('TaskDeleted', callback);
    }
  }

  onReceiveReminder(callback) {
    if (this.connection) {
      this.connection.on('ReceiveReminder', callback);
    }
  }

  onCommentAdded(callback) {
    if (this.connection) {
      this.connection.on('CommentAdded', callback);
    }
  }

  onCommentDeleted(callback) {
    if (this.connection) {
      this.connection.on('CommentDeleted', callback);
    }
  }

  async broadcastTaskUpdate(task) {
    if (this.connection) {
      try {
        await this.connection.invoke('BroadcastTaskUpdate', task);
      } catch (error) {
        console.error('Error broadcasting task update:', error);
      }
    }
  }

  on(methodName, callback) {
    if (this.connection) {
      this.connection.on(methodName, callback);
    }
  }

  off(methodName) {
    if (this.connection) {
      this.connection.off(methodName);
    }
  }
}

export default new SignalRService();