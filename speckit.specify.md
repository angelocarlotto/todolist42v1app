# Speckit Specification: Task Organization Web Application

## Overview
Build a multi-user, multi-tenant web application to help users organize their tasks. The application will allow users to:
- Track what tasks they need to do (To Do)
- Track what tasks they are currently doing (In Progress)
- Track what tasks they have completed (Done)

The application is built with React for the frontend and MongoDB as the database.

## Functional Requirements
1. **User Authentication & Multi-Tenancy**
   - Users must register and log in to access their tasks.
   - Passwords must be exactly 8 characters long and contain only numbers (0-9).
   - Each user belongs to a tenant (organization or workspace).
   - Users can only see and manage tasks within their own tenant.

2. **Task Management**
   - Users can create, edit, and delete tasks.
   - Users can share any task using a public URL with configurable security options:
     - Set expiration time (hours/days)
     - Limit maximum number of views
     - Toggle public editing permissions
     - View live countdown timer showing time until expiration
   - Public viewers see real-time updates via SignalR when tasks are modified
   - Each task has the following properties:
       - ID (unique identifier)
       - Short title
       - Description
       - List of files (attachments or references)
       - Due date
       - Status (To Do, In Progress, Done)
       - Optional tags
       - Criticality (e.g., Low, Medium, High)
       - Public share settings (share ID, expiration, max views, view count, edit permission)
   - Users can move tasks between statuses.
   - Tasks are associated with the user and tenant.
   - Real-time synchronization: When a user updates a task, all other logged-in screens (same user or tenant) reflect the changes immediately via SignalR.

3. **Task Views & Filtering**
   - Users can view tasks by status (To Do, In Progress, Done).
   - The application provides a board interface where users can drag and drop task cards between columns: To Do, Doing, and Done.
   - The color of each task card in the interface must reflect the task's criticality attribute (e.g., red for high, yellow for medium, green for low).
   - Users can filter tasks by due date, tags, or search by keyword.

4. **Multi-User Collaboration**
   - Multiple users within the same tenant can view and manage shared tasks.
   - Task activity (created, updated, completed) is tracked with timestamps and user info.

5. **Notifications & Reminders**
   - Users receive reminders for upcoming or overdue tasks.
   - In-app notifications for task updates (optional).

## Non-Functional Requirements
- **Performance:** Fast UI interactions and efficient database queries.
- **Security:** Secure authentication, authorization, and data isolation between tenants.
- **Scalability:** Support for many users and tenants.
- **Accessibility:** WCAG 2.1 AA compliance.
- **Responsiveness:** Works on desktop and mobile devices.

## Technical Stack & Architecture
- The solution consists of two separate applications:
   - **API (Backend):** C# application (ASP.NET Core) providing a RESTful API for all business logic, authentication, and data access.
   - **Frontend Client:** A React application (JavaScript, no TypeScript) that consumes the API and provides the user interface.
- **Database:** MongoDB (connection string: mongodb://localhost:27017/)
- **Real-time Communication:** SignalR for live updates and collaboration
- **Authentication:** JWT tokens with tenant isolation
- **Testing:** TDD approach using Jest, React Testing Library, and xUnit for C#

## Acceptance Criteria
- Users can register, log in, and manage their own tasks.
- Passwords are exactly 8 digits, numbers only.
- Tasks are visible only to users within the same tenant.
- Users can move tasks between To Do, In Progress, and Done.
- Real-time updates: Task changes reflect immediately across all logged-in user screens via SignalR.
- Public sharing: Users can generate secure public links with configurable expiration, view limits, and edit permissions.
- Public viewers see live countdown timer and real-time task updates.
- Expired or view-limit-exceeded links show appropriate error messages.
- Share dialog pre-populates with existing settings when re-sharing a task.
- The UI is responsive and accessible.
- All core features are covered by automated tests.

---

This specification defines the minimum requirements for the application. All features must adhere to the principles in `speckit.constitution.md`.