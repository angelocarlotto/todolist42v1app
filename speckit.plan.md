# Speckit Plan: Task Organization Web Application

## Technology Stack & Architecture
- The solution consists of two separate applications:
	- **API (Backend):** C# application (ASP.NET Core) providing a RESTful API for all business logic, authentication, and data access.
	- **Frontend Client:** A React application (JavaScript, no TypeScript) that consumes the API and provides the user interface.
- **Database:** MongoDB (mongodb://localhost:27017/)
- **Real-time Communication:** SignalR hub (CollaborationHub) for live task updates and multi-user collaboration

## High-Level Architecture
- The React frontend client communicates with the ASP.NET Core REST API backend.
- MongoDB stores users, tenants, and tasks.
- JWT authentication for secure API access with tenant isolation.
- SignalR WebSocket connections for real-time bidirectional communication between server and clients.

## Key Components & Features

### 1. User Authentication & Multi-Tenancy
- Registration and login forms (React)
- Password validation (exactly 8 digits, numbers only)
- JWT-based authentication
- Tenant selection/creation during registration
- Backend endpoints for user and tenant management

### 2. Task Management
- Task model: ID, short title, description, list of files, due date, status, tags, criticality (Low/Medium/High), user, tenant, public share settings (publicShareId, shareExpiresAt, shareMaxViews, shareViewCount, shareAllowEdit)
- CRUD endpoints for tasks (C# API, ASP.NET Core)
- File upload support (backend and frontend)
- React forms for creating/editing tasks
- SignalR integration: All task CRUD operations broadcast updates to connected clients in real-time

### 3. Board UI (Drag & Drop)
- Board view with columns: To Do, Doing, Done
- React drag-and-drop (e.g., react-beautiful-dnd)
- Cards represent tasks; can be moved between columns
- Card color must reflect the task's criticality (e.g., red for high, yellow for medium, green for low)
- Real-time UI updates via SignalR after drag-and-drop or any task modification

### 4. Task Views & Filtering
- Filter/search tasks by status, due date, tags, criticality, keyword
- Responsive design for desktop and mobile

### 5. Multi-User Collaboration & Real-Time Updates
- Shared tasks within a tenant
- Activity tracking (created, updated, completed by whom/when)
- SignalR hub (CollaborationHub) manages WebSocket connections
- Broadcasting to groups: tenantId, userId, publicShareId
- Authenticated users automatically join tenant and user groups on connection
- Real-time synchronization: If a user is logged in on multiple screens/devices, task updates reflect immediately on all screens

### 6. Notifications & Reminders
- In-app reminders for due/overdue tasks
- Optional: push/email notifications

### 7. Public Task Sharing with Security Controls
- Backend logic (C# API, ASP.NET Core) to generate and manage public share IDs for tasks
- ShareController: Create/revoke shares with configurable options (expiration, max views, allow edit)
- PublicTaskController: Public endpoint to access shared tasks by share ID (no authentication required)
- Share options dialog (React) with fields for:
  - Expiration time (hours and/or days)
  - Maximum view count
  - Allow public editing toggle
- Pre-populate share dialog with existing settings when re-sharing
- Backend validation: Check expiration and view limits on access, increment view count
- Public viewers receive real-time updates via SignalR (separate unauthenticated connection)
- Live countdown timer on public view showing time remaining until expiration (days, hours, minutes, seconds)
- Graceful error handling: Display user-friendly messages for expired links (HTTP 410) or max views exceeded (HTTP 403)
- Conditional edit access: Public viewers can only edit if shareAllowEdit is true

### 8. Testing (TDD)
- Jest and React Testing Library for frontend
- xUnit for C# backend
- Write tests before implementing features

## Milestones
1. ✅ Project setup (React, C# API, MongoDB)
2. ✅ User authentication & multi-tenancy
3. ✅ Task CRUD, file upload, and criticality property
4. ✅ Board UI with drag-and-drop and card color mapping
5. ✅ Filtering, search, and responsive design
6. ✅ Multi-user collaboration features with SignalR real-time updates
7. 🔄 Notifications & reminders (partial - overdue indicators implemented)
8. ✅ Public task sharing with security controls:
   - ✅ Configurable expiration (hours/days)
   - ✅ Maximum view limits with tracking
   - ✅ Conditional edit permissions
   - ✅ Live countdown timer
   - ✅ Real-time updates for public viewers
   - ✅ Pre-populated share dialog for re-sharing
   - ✅ Graceful error handling for expired/exceeded links
9. 🔄 Testing and code coverage (in progress)
10. 🔄 Accessibility and performance improvements (ongoing)
11. 🔄 Deployment and documentation (in progress)

---

All implementation must follow the requirements in `speckit.constitution.md` and `speckit.specify.md`.