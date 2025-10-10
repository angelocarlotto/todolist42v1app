# Task Organization Web Application (todolist42v1app)

A modern, real-time collaborative task management application built with React and ASP.NET Core.

## 🚀 Features

### ✅ Core Task Management
- Create, read, update, and delete tasks
- Task properties: title, description, files, due date, status, tags, criticality
- Three status columns: **To Do**, **In Progress**, **Done**
- Drag-and-drop interface for moving tasks between statuses
- Color-coded task cards based on criticality (High=Red, Medium=Yellow, Low=Green)
- Overdue task indicators

### 👥 Multi-Tenancy & Authentication
- Secure user registration and login with JWT authentication
- Password requirement: exactly 8 digits (numbers only)
- Tenant isolation: users only see tasks within their organization
- Role-based access control

### 🔄 Real-Time Collaboration (SignalR)
- **Multi-screen synchronization**: Task updates reflect instantly across all logged-in screens
- **Live updates**: All CRUD operations broadcast to connected clients in real-time
- **Tenant groups**: Updates only broadcast to users within the same tenant
- **User groups**: Personal updates synchronized across user's devices
- **Public share groups**: Anonymous viewers receive live updates on shared tasks

### 🔗 Public Task Sharing with Security Controls
- Generate secure public share links for any task
- **Configurable expiration**: Set hours and/or days until link expires
- **View limits**: Restrict maximum number of times link can be accessed
- **Edit permissions**: Toggle whether public viewers can edit the task
- **Live countdown timer**: Shows remaining time (days, hours, minutes, seconds) until expiration
- **View count tracking**: Displays current views vs. maximum allowed
- **Graceful error handling**: User-friendly messages for expired or view-limit-exceeded links
- **Pre-populated settings**: Share dialog shows existing settings when re-sharing
- **Real-time updates**: Public viewers see task changes live via SignalR

### 🎨 User Interface
- Responsive design for desktop and mobile
- Intuitive drag-and-drop board interface
- Task filtering and search capabilities
- Loading states and error feedback
- Accessible design (WCAG 2.1 AA compliance goal)

## 🛠️ Technology Stack

### Backend
- **Framework**: ASP.NET Core (.NET 9.0)
- **Database**: MongoDB
- **Real-time**: SignalR (CollaborationHub)
- **Authentication**: JWT tokens with tenant isolation
- **Testing**: xUnit

### Frontend
- **Framework**: React 19.2.0 (JavaScript, no TypeScript)
- **Real-time**: @microsoft/signalr 9.0.6
- **Routing**: React Router 7.9.4
- **HTTP Client**: Axios
- **Date Handling**: date-fns
- **Styling**: CSS modules
- **Testing**: Jest, React Testing Library

## 📁 Project Structure

```
workspacev1/
├── api/                      # ASP.NET Core backend
│   ├── Controllers/
│   │   ├── TasksController.cs         # Task CRUD with SignalR broadcasting
│   │   ├── ShareController.cs         # Share link management
│   │   ├── PublicTaskController.cs    # Public task access (no auth)
│   │   ├── AuthController.cs          # Authentication
│   │   └── TenantsController.cs       # Tenant management
│   ├── Models/
│   │   ├── TaskItem.cs                # Task entity with share properties
│   │   ├── User.cs
│   │   └── Tenant.cs
│   ├── Services/
│   │   ├── TaskService.cs             # MongoDB operations
│   │   └── UserService.cs
│   ├── Hubs/
│   │   └── CollaborationHub.cs        # SignalR hub for real-time updates
│   └── Program.cs                     # App configuration
│
├── client/                   # React frontend
│   ├── src/
│   │   ├── components/
│   │   │   ├── TaskList.js            # Main task board
│   │   │   ├── TaskItem.js            # Task card with share button
│   │   │   ├── ShareOptionsDialog.js  # Share configuration dialog
│   │   │   ├── PublicTaskView.js      # Public task viewer with countdown
│   │   │   ├── Login.js
│   │   │   └── Register.js
│   │   ├── services/
│   │   │   ├── api.js                 # HTTP API client
│   │   │   └── signalr.js             # SignalR service (auth & public)
│   │   ├── App.js
│   │   └── index.js
│   └── package.json
│
├── speckit.constitution.md   # Project principles & standards
├── speckit.specify.md        # Functional requirements
└── speckit.plan.md          # Implementation plan
```

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK
- Node.js (v16+)
- MongoDB (running on mongodb://localhost:27017/)

### Installation

1. **Clone the repository**
   ```bash
   cd c:\Carlotto\todolistapp\todolist42v1app\workspacev1
   ```

2. **Start MongoDB**
   Ensure MongoDB is running on the default port (27017)

3. **Start the Backend API**
   ```bash
   cd api
   dotnet run
   ```
   API will run on: http://localhost:5175

4. **Start the Frontend**
   ```bash
   cd client
   npm install
   npm start
   ```
   App will open on: http://localhost:3000

## 🔐 Authentication

### Registration
- Navigate to registration page
- Create tenant (organization)
- Username: any valid string
- Password: **exactly 8 digits (0-9)**
- Example valid password: `12345678`

### Login
- Use registered credentials
- JWT token stored in localStorage
- Automatic tenant-based data isolation

## 📡 Real-Time Features

### How SignalR Works
1. **Authenticated Users**: 
   - Connect with JWT token on login
   - Automatically join tenant and user groups
   - Receive updates for all tenant tasks

2. **Public Viewers**:
   - Connect without authentication
   - Join specific public share group
   - Receive updates only for shared task

3. **Event Types**:
   - `TaskCreated`: New task added
   - `TaskUpdated`: Task modified or moved
   - `TaskDeleted`: Task removed

### Testing Real-Time Updates
1. Log in as User A on Chrome
2. Log in as User A on Firefox (or incognito)
3. Create/update a task on Chrome
4. See instant update on Firefox ✨

## 🔗 Public Sharing

### Creating a Share Link
1. Click the 🔗 button on any task
2. Configure options:
   - **Expiration**: Hours and/or days until link expires
   - **Max Views**: Limit number of accesses (leave empty for unlimited)
   - **Allow Edit**: Toggle public editing permission
3. Click "Generate Link" - URL copied to clipboard

### Updating Share Settings
1. Click 🔗 on already-shared task
2. Dialog pre-populates with existing settings
3. Modify as needed
4. Click "Update Link" - new link generated

### Viewing Public Links
- Open link in incognito/private window (no login required)
- See countdown timer if expiration is set
- See view count if limit is set
- Edit button only visible if allowed
- Live updates via SignalR

### Share Link Expiration
- **Before expiration**: Shows countdown timer (e.g., "2d 5h 23m 45s")
- **After expiration**: Returns HTTP 410 with friendly error message
- **Max views exceeded**: Returns HTTP 403 with guidance

## 🧪 Testing

### Backend Tests
```bash
cd api.Tests
dotnet test
```

### Frontend Tests
```bash
cd client
npm test
```

## 📋 API Endpoints

### Authenticated Endpoints
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT
- `GET /api/tasks` - Get all tasks for tenant
- `POST /api/tasks` - Create task
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Delete task
- `POST /api/share/{taskId}` - Create share link with options
- `DELETE /api/share/{taskId}` - Revoke share link

### Public Endpoints (No Auth)
- `GET /api/public/task/{publicShareId}` - Get shared task
- `PUT /api/public/task/{publicShareId}` - Update shared task (if allowed)

### SignalR Hub
- `/collaborationHub` - WebSocket endpoint for real-time updates

## 🎯 Current Milestones

- ✅ Project setup (React, ASP.NET Core, MongoDB)
- ✅ User authentication & multi-tenancy
- ✅ Task CRUD with full properties
- ✅ Board UI with drag-and-drop
- ✅ Task filtering and search
- ✅ Real-time collaboration via SignalR
- ✅ Public sharing with security controls
- 🔄 Comprehensive testing (in progress)
- 🔄 Accessibility improvements (ongoing)
- 🔄 Deployment & production optimization

## 📝 Documentation

- **Constitution**: `speckit.constitution.md` - Code quality, testing, UX, performance, security principles
- **Specification**: `speckit.specify.md` - Functional requirements and acceptance criteria
- **Plan**: `speckit.plan.md` - Technical architecture and implementation milestones

## 🤝 Contributing

All contributions must follow the principles in `speckit.constitution.md`:
- Test-Driven Development (TDD)
- 80%+ code coverage
- Accessible design (WCAG 2.1 AA)
- Real-time updates for all multi-user scenarios
- Secure public sharing with proper validation

## 📄 License

This project is part of the todolist42v1app initiative.

## 🐛 Known Issues & Future Enhancements

- [ ] Email/push notifications for task reminders
- [ ] File upload implementation
- [ ] Advanced filtering (by assignee, date range)
- [ ] Task comments and activity log
- [ ] Dark mode theme
- [ ] Mobile app (React Native)
- [ ] Rate limiting for public share endpoints
- [ ] Analytics dashboard for share link usage

---

Built with ❤️ using React, ASP.NET Core, MongoDB, and SignalR
