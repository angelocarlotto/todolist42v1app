import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

class SignalRService {
  constructor() {
    this.connection = null;
  }

  async start() {
    try {
      // If already connected, don't create a new connection
      if (this.connection && this.connection.state === 'Connected') {
        console.log('SignalR already connected, reusing existing connection');
        return this.connection;
      }

      // If connection exists but not connected, stop it first
      if (this.connection) {
        await this.connection.stop();
      }

      const token = localStorage.getItem('authToken');
      const apiUrl = process.env.REACT_APP_API_URL || 'http://10.0.0.71:5175';
      const hubUrl = `${apiUrl}/hub/collaboration`;
      
      this.connection = new HubConnectionBuilder()
        .withUrl(hubUrl, {
          accessTokenFactory: () => token,
          skipNegotiation: false, // Let SignalR negotiate best transport
          transport: undefined    // Use all available transports (WebSockets, ServerSentEvents, LongPolling)
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // Custom retry intervals
        .configureLogging(LogLevel.Information)
        .build();

      // Set up connection lifecycle handlers BEFORE starting
      this.connection.onreconnecting((error) => {
        console.warn('SignalR Reconnecting:', error);
      });

      this.connection.onreconnected((connectionId) => {
        console.log('SignalR Reconnected:', connectionId);
      });

      this.connection.onclose((error) => {
        console.error('SignalR Connection Closed:', error);
      });

      await this.connection.start();
      console.log('SignalR Connected');

      // Wait a bit for connection to be fully ready
      await new Promise(resolve => setTimeout(resolve, 100));

      // Join user's group and tenant group for notifications (only if authenticated)
      const user = JSON.parse(localStorage.getItem('user') || '{}');
      
      if (user.userId) {
        try {
          await this.connection.invoke('JoinGroup', user.userId);
        } catch (error) {
          console.error('Failed to join userId group:', error);
        }
      }
      
      if (user.tenantId) {
        try {
          await this.connection.invoke('JoinGroup', user.tenantId);
        } catch (error) {
          console.error('Failed to join tenantId group:', error);
        }
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
      const apiUrl = process.env.REACT_APP_API_URL || 'http://10.0.0.71:5175';
      const hubUrl = `${apiUrl}/hub/collaboration`;
      
      this.connection = new HubConnectionBuilder()
        .withUrl(hubUrl, {
          skipNegotiation: false,
          transport: undefined
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
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
      this.connection.on('commentAdded', callback);
    }
  }

  onCommentDeleted(callback) {
    if (this.connection) {
      this.connection.on('commentDeleted', callback);
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
