# Speckit Plan: Task Organization Web Application

## Technology Stack & Architecture
- The solution consists of two separate applications:
	- **API (Backend):** C# application (e.g., ASP.NET Core) providing a RESTful API for all business logic, authentication, and data access.
	- **Frontend Client:** A React application (JavaScript, no TypeScript) that consumes the API and provides the user interface.
- **Database:** MongoDB (mongodb://localhost:27017/)

## High-Level Architecture
- The React frontend client communicates with the Node.js/Express REST API backend.
- MongoDB stores users, tenants, and tasks.
- JWT authentication for secure API access.

## Key Components & Features

### 1. User Authentication & Multi-Tenancy
- Registration and login forms (React)
- Password validation (exactly 8 digits, numbers only)
- JWT-based authentication
- Tenant selection/creation during registration
- Backend endpoints for user and tenant management

### 2. Task Management
- Task model: ID, short title, description, list of files, due date, status, tags, criticality (Low/Medium/High), user, tenant, public share ID
- CRUD endpoints for tasks (C# API, ASP.NET Core)
- File upload support (backend and frontend)
- React forms for creating/editing tasks

### 3. Board UI (Drag & Drop)
- Board view with columns: To Do, Doing, Done
- React drag-and-drop (e.g., react-beautiful-dnd)
- Cards represent tasks; can be moved between columns
- Card color must reflect the task's criticality (e.g., red for high, yellow for medium, green for low)
- Real-time UI updates after drag-and-drop

### 4. Task Views & Filtering
- Filter/search tasks by status, due date, tags, criticality, keyword
- Responsive design for desktop and mobile

### 5. Multi-User Collaboration
- Shared tasks within a tenant
- Activity tracking (created, updated, completed by whom/when)

### 6. Notifications & Reminders
- In-app reminders for due/overdue tasks
- Optional: push/email notifications

### 7. Public Task Sharing
- Backend logic (C# API, ASP.NET Core) to generate and manage public share IDs for tasks
- Public endpoint to access shared tasks by share ID, exposing only non-sensitive data
- React UI for generating, copying, and revoking public URLs

### 8. Testing (TDD)
- Jest and React Testing Library for frontend
- xUnit or similar for C# backend
- Write tests before implementing features

## Milestones
1. Project setup (React, C# API, MongoDB)
2. User authentication & multi-tenancy
3. Task CRUD, file upload, and criticality property
4. Board UI with drag-and-drop and card color mapping
5. Filtering, search, and responsive design
6. Multi-user collaboration features
7. Notifications & reminders
8. Public task sharing (backend and frontend)
9. Testing and code coverage
10. Accessibility and performance improvements
11. Deployment and documentation

---

All implementation must follow the requirements in `speckit.constitution.md` and `speckit.specify.md`.