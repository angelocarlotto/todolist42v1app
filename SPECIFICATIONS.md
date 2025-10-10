# TaskFlow - Technical Specifications

**Version:** 1.0  
**Last Updated:** October 10, 2025  
**Document Type:** Technical Specification Document

---

## TABLE OF CONTENTS

1. [System Overview](#1-system-overview)
2. [Functional Requirements](#2-functional-requirements)
3. [Non-Functional Requirements](#3-non-functional-requirements)
4. [System Architecture](#4-system-architecture)
5. [Database Design](#5-database-design)
6. [API Specifications](#6-api-specifications)
7. [Real-Time Communication](#7-real-time-communication)
8. [Security Specifications](#8-security-specifications)
9. [User Interface Specifications](#9-user-interface-specifications)
10. [File Management](#10-file-management)
11. [Error Handling](#11-error-handling)
12. [Testing Requirements](#12-testing-requirements)

---

## 1. SYSTEM OVERVIEW

### 1.1 Purpose
TaskFlow is a real-time collaborative task management system designed for teams and organizations to create, assign, track, and complete tasks with full visibility and communication capabilities.

### 1.2 Scope
- Task CRUD operations with rich metadata
- Real-time collaboration via SignalR
- Multi-tenant architecture with data isolation
- Comment system with activity logging
- File attachment capabilities
- Public task sharing with access controls
- User authentication and authorization

### 1.3 Technology Stack

**Backend:**
- ASP.NET Core 9.0 (Web API)
- MongoDB 7.x
- SignalR for real-time communication
- JWT for authentication

**Frontend:**
- React 19.2.0
- @microsoft/signalr 9.0.6
- Axios for HTTP requests
- date-fns for date formatting
- React Router v6

**Development Environment:**
- Node.js 18+
- .NET SDK 9.0
- MongoDB Community Server
- Visual Studio Code / Visual Studio

---

## 2. FUNCTIONAL REQUIREMENTS

### 2.1 User Authentication

#### FR-AUTH-001: User Login
- **Priority:** Critical
- **Status:** ✅ Implemented
- **Description:** Users must authenticate with username/password to access the system
- **Acceptance Criteria:**
  - User provides valid credentials
  - System validates against database
  - JWT token generated with user and tenant claims
  - Token stored in client localStorage
  - User redirected to main application view
  - Invalid credentials show error message

#### FR-AUTH-002: User Registration
- **Priority:** High
- **Status:** ❌ Not Implemented
- **Description:** New users can create accounts within a tenant
- **Acceptance Criteria:**
  - User provides username, password, email
  - Password meets complexity requirements
  - Username unique within tenant
  - Account created and user can log in immediately

#### FR-AUTH-003: Password Management
- **Priority:** Medium
- **Status:** ❌ Not Implemented
- **Description:** Users can reset/change passwords
- **Acceptance Criteria:**
  - Forgot password flow with email verification
  - Change password from user settings
  - Old password validation required

### 2.2 Task Management

#### FR-TASK-001: Create Task
- **Priority:** Critical
- **Status:** ✅ Implemented
- **Description:** Users can create new tasks with required and optional fields
- **Input Fields:**
  - Short Title (required, max 100 chars)
  - Description (required, max 2000 chars)
  - Due Date (required)
  - Status (required: ToDo, InProgress, Done)
  - Criticality (required: Low, Medium, High)
  - Tags (optional, array)
  - Assigned Users (optional, array of user IDs)
- **Business Rules:**
  - Creator automatically tracked (createdBy)
  - Tenant ID from JWT claims
  - Initial activity log entry created
  - Real-time broadcast to all tenant users
- **Acceptance Criteria:**
  - Task created in database
  - Task appears immediately in UI for all users
  - Validation errors shown for invalid input
  - Success notification displayed

#### FR-TASK-002: Update Task
- **Priority:** Critical
- **Status:** ✅ Implemented
- **Description:** Users can modify existing tasks
- **Editable Fields:** All fields from create except createdBy, createdAt
- **Business Rules:**
  - UpdatedBy and UpdatedAt automatically set
  - Status change to "Done" sets completedBy and completedAt
  - Activity log entry created for changes
  - Real-time broadcast to all tenant users
- **Acceptance Criteria:**
  - Changes persisted to database
  - UI updates immediately for all users
  - Activity log shows what changed
  - Previous values preserved in activity log for status changes

#### FR-TASK-003: Delete Task
- **Priority:** Critical
- **Status:** ✅ Implemented
- **Description:** Users can permanently delete tasks
- **Business Rules:**
  - Confirmation required before deletion
  - Associated files deleted from storage
  - Real-time broadcast to all tenant users
- **Acceptance Criteria:**
  - Task removed from database
  - Task disappears from UI for all users
  - Confirmation dialog shown before delete
  - Cannot be undone (no soft delete)

#### FR-TASK-004: View Task List
- **Priority:** Critical
- **Status:** ✅ Implemented
- **Description:** Users see all tasks within their tenant
- **Display:**
  - Grid layout with task cards
  - Status indicator (color-coded)
  - Priority indicator
  - Due date with overdue warning
  - Assigned users count
  - File count
  - Comment count
  - Activity count
- **Acceptance Criteria:**
  - Only tenant tasks shown
  - Real-time updates when tasks change
  - Overdue tasks visually distinct

#### FR-TASK-005: Task Board (Kanban)
- **Priority:** High
- **Status:** ✅ Implemented
- **Description:** Visual board showing tasks grouped by status
- **Columns:** ToDo, InProgress, Done
- **Features:**
  - Task count per column
  - Drag-and-drop to change status (future)
- **Acceptance Criteria:**
  - Tasks appear in correct column based on status
  - Counts update in real-time

#### FR-TASK-006: Filter Tasks
- **Priority:** High
- **Status:** ✅ Partial (status filter only)
- **Description:** Users can filter visible tasks
- **Filter Options:**
  - ✅ All tasks
  - ✅ By status (ToDo, InProgress, Done)
  - ❌ By assigned user
  - ❌ By tag
  - ❌ By priority
  - ❌ By date range
- **Acceptance Criteria:**
  - Filter persists during session
  - Filter applies immediately
  - Clear visual indication of active filters

#### FR-TASK-007: Sort Tasks
- **Priority:** High
- **Status:** ✅ Implemented
- **Description:** Users can sort task list
- **Sort Options:**
  - By due date (ascending)
  - By priority (High → Low)
  - By status
  - By title (alphabetical)
- **Acceptance Criteria:**
  - Sort applies immediately
  - Sort persists during session
  - Visual indication of active sort

#### FR-TASK-008: Search Tasks
- **Priority:** High
- **Status:** ❌ Not Implemented
- **Description:** Users can search tasks by text
- **Search Fields:** Title, description, tags, comments
- **Acceptance Criteria:**
  - Real-time search as user types
  - Highlights matching text
  - Search works with filters

### 2.3 User Assignment

#### FR-ASSIGN-001: Assign Users to Task
- **Priority:** High
- **Status:** ✅ Implemented (backend only)
- **Description:** Add users to task's assigned list
- **Endpoint:** `POST /api/tasks/{id}/assign`
- **Business Rules:**
  - Only users within same tenant
  - Activity log entry created
  - Real-time notification to assigned users
- **Acceptance Criteria:**
  - Users added to assignedUsers array
  - Activity log shows assignment
  - Assigned users notified

#### FR-ASSIGN-002: Unassign Users from Task
- **Priority:** High
- **Status:** ✅ Implemented (backend only)
- **Description:** Remove users from task's assigned list
- **Endpoint:** `POST /api/tasks/{id}/unassign`
- **Acceptance Criteria:**
  - Users removed from assignedUsers array
  - Activity log shows unassignment

### 2.4 Comments System

#### FR-COMMENT-001: Add Comment
- **Priority:** High
- **Status:** ✅ Implemented
- **Description:** Users can add text comments to tasks
- **Input:** Comment text (required, max 1000 chars)
- **Business Rules:**
  - Username and userId automatically captured
  - Timestamp automatically set
  - Real-time broadcast via SignalR
  - Task refreshed in UI for all users
- **Acceptance Criteria:**
  - Comment added to task.comments array
  - Comment visible immediately to all users viewing task
  - Username displayed with comment
  - Timestamp shown

#### FR-COMMENT-002: Delete Comment
- **Priority:** Medium
- **Status:** ✅ Implemented
- **Description:** Users can delete their own comments
- **Business Rules:**
  - Only comment author can delete
  - Real-time broadcast via SignalR
  - Task refreshed in UI
- **Acceptance Criteria:**
  - Comment removed from array
  - UI updates immediately for all users
  - Delete button only visible to comment author

#### FR-COMMENT-003: View Comments
- **Priority:** High
- **Status:** ✅ Implemented
- **Description:** Comments displayed in expandable section
- **Display:**
  - Comment count in header
  - Expand/collapse button
  - List of comments with user and timestamp
  - Delete button for owned comments
- **Acceptance Criteria:**
  - Comments sorted by timestamp (oldest first)
  - Section collapsed by default
  - Count updates in real-time

### 2.5 Activity Log

#### FR-ACTIVITY-001: Automatic Activity Tracking
- **Priority:** High
- **Status:** ✅ Implemented
- **Description:** System automatically logs task changes
- **Logged Activities:**
  - Task created
  - Task updated
  - Status changed (with old/new values)
  - User assigned
  - User unassigned
  - File uploaded
  - File deleted
- **Business Rules:**
  - Username and userId captured automatically
  - Timestamp set automatically
  - Old/new values stored for status changes
- **Acceptance Criteria:**
  - Activity entries created automatically
  - No manual intervention required
  - Activities include descriptive text

#### FR-ACTIVITY-002: View Activity Log
- **Priority:** Medium
- **Status:** ✅ Implemented
- **Description:** Users can view task activity history
- **Display:**
  - Activity count in header
  - Expand/collapse button
  - Chronological list of activities
  - Activity type, description, user, timestamp
- **Acceptance Criteria:**
  - Activities sorted by timestamp (newest first)
  - Section collapsed by default
  - Updates in real-time

### 2.6 File Attachments

#### FR-FILE-001: Upload Files
- **Priority:** High
- **Status:** ✅ Implemented
- **Description:** Users can attach files to tasks
- **Features:**
  - Multiple file selection
  - Automatic upload on selection (no manual submit)
  - Support for various file types
- **Business Rules:**
  - Files stored in /uploads directory
  - File path stored in task.files array
  - Real-time broadcast of task update
  - ⚠️ No file size limit currently enforced
  - ⚠️ No file type validation
- **Acceptance Criteria:**
  - Files uploaded to server
  - Files appear immediately in UI for all users
  - File icons based on type
  - Download links functional

#### FR-FILE-002: Delete Files
- **Priority:** Medium
- **Status:** ✅ Implemented
- **Description:** Users can remove attached files
- **Business Rules:**
  - Confirmation required
  - Physical file deleted from storage
  - File path removed from task.files array
  - Real-time broadcast of task update
- **Acceptance Criteria:**
  - File removed from storage and database
  - File disappears from UI for all users
  - Confirmation dialog shown
  - Handles missing files gracefully

#### FR-FILE-003: Download Files
- **Priority:** High
- **Status:** ✅ Implemented
- **Description:** Users can download attached files
- **Features:**
  - Click filename to download
  - Files served via /uploads static route
- **Acceptance Criteria:**
  - Files downloadable via link
  - Original filename preserved
  - Browser handles file appropriately

### 2.7 Public Sharing

#### FR-SHARE-001: Generate Public Link
- **Priority:** Medium
- **Status:** ✅ Implemented
- **Description:** Users can create public share links for tasks
- **Options:**
  - Expiration time (hours/days)
  - Maximum view count
  - Allow editing permission
- **Business Rules:**
  - Unique publicShareId generated
  - Share options stored in task
  - Link copied to clipboard automatically
  - Share can be revoked and recreated
- **Acceptance Criteria:**
  - Public link generated
  - Link copied to clipboard
  - Success message shown
  - Link opens task in public view

#### FR-SHARE-002: View Public Task
- **Priority:** Medium
- **Status:** ✅ Implemented
- **Description:** External users can view shared tasks
- **Route:** `/public/task/{publicShareId}`
- **Validation:**
  - Check expiration date
  - Check view count limit
  - Increment view count
- **Display:**
  - Read-only task details
  - Optional: Edit capability if shareAllowEdit is true
- **Acceptance Criteria:**
  - Task visible without authentication
  - Expired links show error
  - Max views exceeded shows error
  - View count incremented

#### FR-SHARE-003: Revoke Share
- **Priority:** Medium
- **Status:** ✅ Implemented
- **Description:** Users can revoke public access
- **Business Rules:**
  - Removes publicShareId from task
  - Previous link no longer works
- **Acceptance Criteria:**
  - Public link becomes invalid
  - Attempting to access shows error

### 2.8 Notifications

#### FR-NOTIFY-001: In-App Notifications
- **Priority:** Medium
- **Status:** ✅ Implemented (basic)
- **Description:** Users see notifications for actions
- **Notification Types:**
  - Task created (success)
  - Task updated (info)
  - Task deleted (warning)
  - Reminders (warning)
- **Display:**
  - Toast-style notifications
  - Auto-dismiss after 5 seconds
  - Notification count badge in navbar
- **Acceptance Criteria:**
  - Notifications appear for actions
  - Notifications dismissable
  - Multiple notifications queue

#### FR-NOTIFY-002: Email Notifications
- **Priority:** Low
- **Status:** ❌ Not Implemented
- **Description:** Users receive email for important events

#### FR-NOTIFY-003: Push Notifications
- **Priority:** Low
- **Status:** ❌ Not Implemented
- **Description:** Browser push notifications for updates

### 2.9 Tenant Management

#### FR-TENANT-001: List Tenant Users
- **Priority:** High
- **Status:** ✅ Implemented (backend only)
- **Description:** View all users in tenant
- **Endpoint:** `GET /api/tenants/users`
- **Acceptance Criteria:**
  - Returns users for authenticated user's tenant
  - Excludes password hashes
  - Real user data returned

#### FR-TENANT-002: Add User to Tenant
- **Priority:** Medium
- **Status:** ✅ Implemented (backend only)
- **Description:** Admin can add users to tenant
- **Endpoint:** `POST /api/tenants/users`

---

## 3. NON-FUNCTIONAL REQUIREMENTS

### 3.1 Performance

#### NFR-PERF-001: API Response Time
- **Requirement:** 95% of API requests complete within 200ms
- **Status:** ⏳ Not measured
- **Testing:** Load testing with 100 concurrent requests

#### NFR-PERF-002: Real-Time Latency
- **Requirement:** SignalR messages delivered within 100ms
- **Status:** ✅ Subjectively fast
- **Testing:** Measure time from server broadcast to client update

#### NFR-PERF-003: Initial Load Time
- **Requirement:** Application loads within 2 seconds on 4G connection
- **Status:** ⏳ Not measured
- **Testing:** Lighthouse performance audit

#### NFR-PERF-004: Concurrent Users
- **Requirement:** Support 100+ concurrent users per tenant
- **Status:** ⏳ Not tested
- **Testing:** Load testing with simulated concurrent connections

### 3.2 Security

#### NFR-SEC-001: Authentication Required
- **Requirement:** All endpoints except public share require authentication
- **Status:** ✅ Implemented
- **Validation:** JWT token validation on all requests

#### NFR-SEC-002: Tenant Isolation
- **Requirement:** Users cannot access data outside their tenant
- **Status:** ✅ Implemented
- **Validation:** TenantId validated from JWT claims on every request

#### NFR-SEC-003: Input Sanitization
- **Requirement:** All user input sanitized to prevent XSS
- **Status:** ✅ Partial (HtmlEncode on backend)
- **Improvement Needed:** Client-side sanitization, CSP headers

#### NFR-SEC-004: Password Security
- **Requirement:** Passwords hashed with industry-standard algorithm
- **Status:** ✅ Implemented (bcrypt/PBKDF2)
- **Validation:** Never store plain-text passwords

#### NFR-SEC-005: HTTPS
- **Requirement:** All traffic encrypted in production
- **Status:** ❌ Not configured (dev only)
- **Deployment:** Configure SSL certificates

### 3.3 Usability

#### NFR-USE-001: Responsive Design
- **Requirement:** Application usable on desktop, tablet, mobile
- **Status:** ⏳ Partial (desktop-optimized)
- **Testing:** Test on various screen sizes

#### NFR-USE-002: Intuitive Interface
- **Requirement:** New users can create task within 30 seconds
- **Status:** ✅ Subjectively easy
- **Testing:** User testing with first-time users

#### NFR-USE-003: Accessibility
- **Requirement:** WCAG 2.1 Level AA compliance
- **Status:** ❌ Not tested
- **Testing:** Accessibility audit, screen reader testing

### 3.4 Reliability

#### NFR-REL-001: Uptime
- **Requirement:** 99% uptime in production
- **Status:** ⏳ Not applicable (dev)
- **Monitoring:** Uptime monitoring service

#### NFR-REL-002: Data Integrity
- **Requirement:** No data loss during operations
- **Status:** ✅ Database transactions ensure consistency
- **Validation:** Regular backups, transaction logging

#### NFR-REL-003: Error Handling
- **Requirement:** Graceful error handling with user-friendly messages
- **Status:** ✅ Partial (some errors not handled)
- **Improvement:** Comprehensive error boundaries

### 3.5 Maintainability

#### NFR-MAIN-001: Code Documentation
- **Requirement:** All public APIs documented
- **Status:** ❌ Limited inline comments
- **Improvement:** Add XML documentation, JSDoc comments

#### NFR-MAIN-002: Code Quality
- **Requirement:** Follow language-specific best practices
- **Status:** ✅ Generally good
- **Tools:** ESLint, StyleCop (future)

#### NFR-MAIN-003: Automated Tests
- **Requirement:** 80% code coverage
- **Status:** ❌ No tests currently
- **Implementation:** Unit tests, integration tests

### 3.6 Scalability

#### NFR-SCALE-001: Horizontal Scaling
- **Requirement:** Application can scale across multiple servers
- **Status:** ⏳ SignalR needs sticky sessions or Redis backplane
- **Implementation:** Configure Redis for SignalR

#### NFR-SCALE-002: Database Scaling
- **Requirement:** Database can handle growing data
- **Status:** ⏳ MongoDB supports sharding
- **Implementation:** Add indexes, consider sharding strategy

---

## 4. SYSTEM ARCHITECTURE

### 4.1 High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         CLIENT TIER                          │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  React Application (Port 3000)                         │ │
│  │  - Components (TaskList, TaskItem, TaskForm, etc.)    │ │
│  │  - Context API (AppContext)                            │ │
│  │  - Services (API, SignalR)                             │ │
│  │  - Routing (React Router)                              │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                            ↓↑ HTTP/WebSocket
┌─────────────────────────────────────────────────────────────┐
│                       API TIER                               │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  ASP.NET Core Web API (Port 5175)                      │ │
│  │  - Controllers (Tasks, Auth, Comments, Share)          │ │
│  │  - Services (TaskService, UserService)                 │ │
│  │  - SignalR Hub (TaskHub)                               │ │
│  │  - JWT Authentication                                   │ │
│  │  - Static Files (/uploads)                             │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                            ↓↑ MongoDB Driver
┌─────────────────────────────────────────────────────────────┐
│                      DATA TIER                               │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  MongoDB Database                                       │ │
│  │  - tasks collection                                     │ │
│  │  - users collection                                     │ │
│  │  - tenants collection                                   │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### 4.2 Component Architecture

**Frontend Components:**
```
App
├── AppProvider (Context)
├── Router
    ├── AppContent
    │   ├── Navbar
    │   ├── TaskBoard
    │   ├── TaskList
    │   │   └── TaskItem (multiple)
    │   │       ├── ShareOptionsDialog
    │   │       ├── CommentsSection
    │   │       ├── ActivityLog
    │   │       └── FileAttachments
    │   └── Notifications
    ├── Login
    └── PublicTaskView
```

**Backend Structure:**
```
api/
├── Controllers/
│   ├── AuthController.cs
│   ├── TasksController.cs
│   ├── CommentsController.cs
│   ├── ShareController.cs
│   ├── TenantsController.cs
│   └── PublicTaskController.cs
├── Models/
│   ├── TaskItem.cs
│   ├── User.cs
│   ├── Tenant.cs
│   ├── Comment.cs
│   ├── ActivityLog.cs
│   └── DatabaseSettings.cs
├── Services/
│   ├── TaskService.cs
│   ├── UserService.cs
│   └── TenantService.cs
├── Hubs/
│   └── TaskHub.cs
└── Program.cs
```

### 4.3 Data Flow

**Task Creation Flow:**
```
1. User fills TaskForm → clicks Submit
2. AppContext.createTask() called
3. api.createTask() → POST /api/tasks
4. TasksController validates tenant, creates task
5. TaskService.CreateAsync() saves to MongoDB
6. HubContext broadcasts "TaskCreated" to tenant group
7. All clients receive SignalR event
8. AppContext.onTaskCreated() → dispatch ADD_TASK
9. UI updates with new task
```

**Real-Time Update Flow:**
```
1. User A updates task
2. Server broadcasts TaskUpdated via SignalR
3. User A, B, C all receive event
4. Each client updates local state
5. UI re-renders with new data
```

---

## 5. DATABASE DESIGN

### 5.1 Collections

#### tasks Collection
```json
{
  "_id": ObjectId,
  "shortTitle": String (required, max 100),
  "description": String (required, max 2000),
  "files": [String], // Array of file paths
  "dueDate": ISODate (required),
  "status": String (enum: "ToDo", "InProgress", "Done"),
  "tags": [String],
  "criticality": String (enum: "Low", "Medium", "High"),
  "userId": ObjectId (creator),
  "assignedUsers": [ObjectId],
  "tenantId": ObjectId (required, indexed),
  "publicShareId": String (unique, sparse index),
  "shareExpiresAt": ISODate,
  "shareMaxViews": Number,
  "shareViewCount": Number (default: 0),
  "shareAllowEdit": Boolean (default: false),
  "comments": [
    {
      "_id": ObjectId,
      "userId": String,
      "username": String,
      "text": String,
      "createdAt": ISODate
    }
  ],
  "activityLog": [
    {
      "_id": ObjectId,
      "userId": String,
      "username": String,
      "activityType": String,
      "description": String,
      "oldValue": String,
      "newValue": String,
      "timestamp": ISODate
    }
  ],
  "createdAt": ISODate (default: now),
  "createdBy": String,
  "updatedAt": ISODate (default: now),
  "updatedBy": String,
  "completedBy": String,
  "completedAt": ISODate
}
```

#### users Collection
```json
{
  "_id": ObjectId,
  "username": String (unique, required),
  "passwordHash": String (required),
  "tenantId": ObjectId (required, indexed),
  "role": String (default: "user"),
  "email": String,
  "createdAt": ISODate (default: now)
}
```

#### tenants Collection
```json
{
  "_id": ObjectId,
  "name": String (required),
  "createdAt": ISODate (default: now)
}
```

### 5.2 Indexes

**Required Indexes:**
```javascript
// tasks collection
db.tasks.createIndex({ "tenantId": 1 })
db.tasks.createIndex({ "publicShareId": 1 }, { unique: true, sparse: true })
db.tasks.createIndex({ "dueDate": 1 })
db.tasks.createIndex({ "status": 1 })

// users collection
db.users.createIndex({ "username": 1 }, { unique: true })
db.users.createIndex({ "tenantId": 1 })

// tenants collection
db.tenants.createIndex({ "name": 1 })
```

**Performance Optimization Indexes:**
```javascript
// Compound index for common queries
db.tasks.createIndex({ "tenantId": 1, "status": 1, "dueDate": 1 })
db.tasks.createIndex({ "tenantId": 1, "assignedUsers": 1 })

// Text index for search (future)
db.tasks.createIndex({ "shortTitle": "text", "description": "text" })
```

---

## 6. API SPECIFICATIONS

### 6.1 Authentication APIs

#### POST /api/auth/login
**Description:** Authenticate user and receive JWT token

**Request:**
```json
{
  "username": "john.doe",
  "password": "SecurePassword123!"
}
```

**Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "507f1f77bcf86cd799439011",
    "username": "john.doe",
    "tenantId": "507f191e810c19729de860ea"
  }
}
```

**Response (401):**
```json
{
  "message": "Invalid credentials"
}
```

#### GET /api/auth/me
**Description:** Get current authenticated user info

**Headers:** `Authorization: Bearer {token}`

**Response (200):**
```json
{
  "id": "507f1f77bcf86cd799439011",
  "username": "john.doe",
  "tenantId": "507f191e810c19729de860ea"
}
```

### 6.2 Task APIs

#### GET /api/tasks
**Description:** Get all tasks for authenticated user's tenant

**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
- `status` (optional): Filter by status
- `assignedTo` (optional): Filter by assigned user (future)

**Response (200):**
```json
[
  {
    "id": "507f1f77bcf86cd799439011",
    "shortTitle": "Implement user registration",
    "description": "Create registration form and API endpoint",
    "status": "InProgress",
    "criticality": "High",
    "dueDate": "2025-10-15T00:00:00Z",
    "tags": ["backend", "authentication"],
    "files": ["/uploads/spec-doc.pdf"],
    "assignedUsers": ["507f1f77bcf86cd799439012"],
    "comments": [...],
    "activityLog": [...],
    "createdAt": "2025-10-10T08:00:00Z",
    "createdBy": "john.doe",
    "updatedAt": "2025-10-10T10:30:00Z",
    "updatedBy": "jane.smith"
  }
]
```

#### GET /api/tasks/{id}
**Description:** Get specific task by ID

**Headers:** `Authorization: Bearer {token}`

**Response (200):** Single task object (same structure as array item above)

**Response (404):**
```json
{
  "message": "Task not found"
}
```

#### POST /api/tasks
**Description:** Create new task

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "shortTitle": "New Feature",
  "description": "Detailed description",
  "dueDate": "2025-10-20T00:00:00Z",
  "status": "ToDo",
  "criticality": "Medium",
  "tags": ["feature", "frontend"],
  "assignedUsers": []
}
```

**Response (201):** Created task object with generated ID

**Response (400):**
```json
{
  "message": "ShortTitle is required"
}
```

#### PUT /api/tasks/{id}
**Description:** Update existing task

**Headers:** `Authorization: Bearer {token}`

**Request:** Full task object with updated fields

**Response (200):** Updated task object

**Response (404):** Task not found

#### DELETE /api/tasks/{id}
**Description:** Delete task permanently

**Headers:** `Authorization: Bearer {token}`

**Response (204):** No content

**Response (404):** Task not found

### 6.3 File APIs

#### POST /api/tasks/{id}/upload
**Description:** Upload files to task

**Headers:** 
- `Authorization: Bearer {token}`
- `Content-Type: multipart/form-data`

**Request:** FormData with files

**Response (200):**
```json
{
  "files": [
    "/uploads/document-1728563247123.pdf",
    "/uploads/image-1728563247456.png"
  ]
}
```

#### DELETE /api/tasks/{id}/files
**Description:** Delete file from task

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "filePath": "/uploads/document-1728563247123.pdf"
}
```

**Response (200):**
```json
{
  "files": [
    "/uploads/image-1728563247456.png"
  ]
}
```

### 6.4 Comment APIs

#### POST /api/comments/{taskId}
**Description:** Add comment to task

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "text": "This looks good, let's proceed"
}
```

**Response (200):**
```json
{
  "id": "507f1f77bcf86cd799439099",
  "userId": "507f1f77bcf86cd799439011",
  "username": "john.doe",
  "text": "This looks good, let's proceed",
  "createdAt": "2025-10-10T12:00:00Z"
}
```

#### DELETE /api/comments/{taskId}/{commentId}
**Description:** Delete comment from task

**Headers:** `Authorization: Bearer {token}`

**Response (200):**
```json
{
  "message": "Comment deleted successfully"
}
```

### 6.5 Share APIs

#### POST /api/share/{taskId}
**Description:** Create public share link

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "maxViews": 100,
  "expiresInHours": 48,
  "allowEdit": false
}
```

**Response (200):**
```json
{
  "publicShareId": "a7f3e9c2-4b1d-5e8f-9a2c-6d4e8f1a3b5c",
  "expiresAt": "2025-10-12T12:00:00Z",
  "shareUrl": "http://localhost:3000/public/task/a7f3e9c2-4b1d-5e8f-9a2c-6d4e8f1a3b5c"
}
```

#### DELETE /api/share/{taskId}
**Description:** Revoke public share

**Headers:** `Authorization: Bearer {token}`

**Response (200):**
```json
{
  "message": "Share revoked successfully"
}
```

#### GET /api/public/task/{publicShareId}
**Description:** Get task via public share link (no auth required)

**Response (200):** Task object

**Response (403):**
```json
{
  "message": "Share link has expired"
}
```
or
```json
{
  "message": "Maximum view count reached"
}
```

---

## 7. REAL-TIME COMMUNICATION

### 7.1 SignalR Hub

**Hub Name:** `TaskHub`  
**Connection URL:** `http://localhost:5175/taskhub`

### 7.2 Server → Client Events

#### TaskCreated
**Payload:** Complete TaskItem object  
**Trigger:** New task created via POST /api/tasks  
**Recipients:** All users in task's tenant group

#### TaskUpdated
**Payload:** Complete updated TaskItem object  
**Trigger:** Task updated via PUT /api/tasks, file upload/delete  
**Recipients:** All users in task's tenant group

#### TaskDeleted
**Payload:** String (taskId)  
**Trigger:** Task deleted via DELETE /api/tasks  
**Recipients:** All users in task's tenant group

#### CommentAdded
**Payload:** `{ taskId: string, commentId: string }`  
**Trigger:** Comment added via POST /api/comments  
**Recipients:** All users in task's tenant group

#### CommentDeleted
**Payload:** `{ taskId: string, commentId: string }`  
**Trigger:** Comment deleted via DELETE /api/comments  
**Recipients:** All users in task's tenant group

#### ReceiveReminder
**Payload:** Reminder object  
**Trigger:** Scheduled reminder check (future)  
**Recipients:** Assigned users

### 7.3 Connection Management

**Authentication:**
- JWT token sent as query parameter: `?access_token={token}`
- Token validated on connection

**Groups:**
- User automatically added to tenant group on connection
- Group name: `{tenantId}`
- Public share tasks: `public_{publicShareId}`

**Reconnection:**
- Automatic reconnection with exponential backoff
- Max 5 retry attempts
- State preserved across reconnections

---

## 8. SECURITY SPECIFICATIONS

### 8.1 Authentication Flow

1. User submits credentials
2. Server validates against database
3. Password verified with hash comparison
4. JWT token generated with claims:
   - `userId`: User's database ID
   - `username`: User's username
   - `tenantId`: User's tenant ID
   - `exp`: Expiration timestamp (24 hours)
5. Token returned to client
6. Client stores in localStorage
7. Client includes in Authorization header: `Bearer {token}`

### 8.2 Authorization Rules

**Task Operations:**
- User must be authenticated
- User's tenantId must match task's tenantId
- Enforced by controller checking JWT claims

**Comment Operations:**
- Add: Any authenticated user in tenant
- Delete: Only comment author

**File Operations:**
- Upload: Any authenticated user in tenant
- Delete: Any authenticated user in tenant
- Download: Any authenticated user (via /uploads static files)

**Public Share:**
- View: No authentication (if not expired/max views)
- Edit: Only if shareAllowEdit is true

### 8.3 Input Validation

**Backend Validation:**
```csharp
// Required fields
if (string.IsNullOrWhiteSpace(task.ShortTitle)) 
    return BadRequest("ShortTitle is required");

// Enum validation
var allowedStatus = new[] { "ToDo", "InProgress", "Done" };
if (!allowedStatus.Contains(task.Status)) 
    return BadRequest($"Status must be one of: {string.Join(", ", allowedStatus)}");

// Sanitization
task.ShortTitle = System.Net.WebUtility.HtmlEncode(task.ShortTitle.Trim());
```

**Frontend Validation:**
- Required field checks
- Max length enforcement
- Date validation
- Enum dropdown (prevents invalid values)

### 8.4 Vulnerabilities & Mitigations

| Vulnerability | Risk Level | Current Status | Mitigation |
|---------------|------------|----------------|------------|
| XSS | Medium | ✅ Partial | HtmlEncode on backend, React auto-escapes |
| CSRF | Low | ✅ Mitigated | SPA architecture, JWT tokens |
| SQL Injection | N/A | ✅ N/A | MongoDB (NoSQL) |
| NoSQL Injection | Medium | ⚠️ Possible | Validate input types, use parameterized queries |
| Broken Auth | Medium | ✅ Mitigated | JWT with claims, validation on every request |
| Sensitive Data | High | ⚠️ Risk | HTTPS not enabled (dev only) |
| Missing Auth | Low | ✅ Mitigated | [Authorize] on all endpoints |
| File Upload | High | ⚠️ Risk | No file type/size validation |
| Rate Limiting | Medium | ❌ Missing | Implement rate limiting middleware |

---

## 9. USER INTERFACE SPECIFICATIONS

### 9.1 Layout Structure

**Main Application:**
```
┌─────────────────────────────────────────────────────────┐
│ Navbar: Logo | User Info | Notifications | Logout      │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  Task Board: [ToDo] [InProgress] [Done] (counts)       │
│                                                          │
│  ┌────────────────────────────────────────────────────┐ │
│  │ Task List Header: "My Tasks" [+ New Task Button]   │ │
│  │                                                      │ │
│  │ Filters: [All|ToDo|InProgress|Done]                │ │
│  │ Sort: [Due Date|Priority|Status|Title]             │ │
│  │                                                      │ │
│  │ ┌────────┐ ┌────────┐ ┌────────┐                  │ │
│  │ │ Task 1 │ │ Task 2 │ │ Task 3 │  ...             │ │
│  │ └────────┘ └────────┘ └────────┘                  │ │
│  └────────────────────────────────────────────────────┘ │
│                                                          │
├─────────────────────────────────────────────────────────┤
│ Notifications: Toast messages                           │
└─────────────────────────────────────────────────────────┘
```

### 9.2 Task Card Layout

```
┌─────────────────────────────────────────────────────────┐
│ ┌─ Task Title ──────────────────────┐ [✏️] [🗑️] [🔗] │
│ │                                                        │
│ │ Description text here...                             │
│ │                                                        │
│ │ [Status Badge] [Priority Badge]                      │
│ │                                                        │
│ │ Due: Oct 15, 2025 ⚠️ Overdue                         │
│ │ Created: Oct 10, 2025                                │
│ │                                                        │
│ │ Tags: [backend] [auth]                               │
│ │ Assigned: 2 user(s)                                  │
│ │                                                        │
│ │ Files: 📄 document.pdf [❌] 🖼️ image.png [❌]       │
│ │                                                        │
│ │ ▼ Comments (3)                                        │
│ │ ▼ Activity (5)                                        │
│ └──────────────────────────────────────────────────────┘
└─────────────────────────────────────────────────────────┘
```

### 9.3 Color Scheme

**Status Colors:**
- ToDo: `#3b82f6` (blue)
- InProgress: `#f59e0b` (amber/orange)
- Done: `#10b981` (green)

**Priority Colors:**
- High: `#ef4444` (red)
- Medium: `#f59e0b` (amber)
- Low: `#10b981` (green)

**UI Colors:**
- Primary: `#3b82f6` (blue)
- Secondary: `#6b7280` (gray)
- Success: `#10b981` (green)
- Warning: `#f59e0b` (amber)
- Danger: `#ef4444` (red)
- Background: `#f9fafb` (light gray)
- Card: `#ffffff` (white)
- Border: `#e5e7eb` (light gray)

### 9.4 Responsive Breakpoints

- Desktop: `>= 1024px` (grid layout, 3 columns)
- Tablet: `768px - 1023px` (grid layout, 2 columns)
- Mobile: `< 768px` (single column, stacked)

### 9.5 Component States

**Button States:**
- Default: Solid color
- Hover: Slightly darker
- Active: Even darker
- Disabled: Gray with 50% opacity
- Loading: Spinner icon

**Task Card States:**
- Default: White background
- Hover: Slight shadow elevation
- Overdue: Red border
- Selected: Blue border (future)

### 9.6 Modal Dialogs

**TaskForm Modal:**
- Overlay: Semi-transparent black background
- Modal: Centered, white background, max-width 600px
- Fields: All input fields stacked vertically
- Buttons: Cancel (left) | Create/Update (right)

**ShareOptionsDialog:**
- Similar styling to TaskForm
- Fields: Expiration, Max Views, Allow Edit checkbox
- Preview of share link

---

## 10. FILE MANAGEMENT

### 10.1 Upload Process

1. User clicks "Upload Files" button
2. File picker opens (native browser dialog)
3. User selects one or more files
4. Files immediately uploaded via POST to `/api/tasks/{id}/upload`
5. Server generates unique filename: `{originalName}-{timestamp}{extension}`
6. Files saved to `/uploads` directory
7. File paths added to task.files array
8. TaskUpdated broadcast via SignalR
9. UI updates showing new files

### 10.2 File Storage

**Directory Structure:**
```
/uploads
  ├── document-1728563247123.pdf
  ├── image-1728563247456.png
  ├── spreadsheet-1728563300789.xlsx
  └── ...
```

**Filename Format:** `{originalName}-{unixTimestamp}.{extension}`

**Static File Serving:**
```csharp
app.UseStaticFiles(new StaticFileOptions {
    FileProvider = new PhysicalFileProvider(Path.Combine("uploads")),
    RequestPath = "/uploads"
});
```

### 10.3 File Download

**URL Format:** `http://localhost:5175/uploads/{filename}`

**Browser Behavior:**
- PDFs: Open in browser viewer
- Images: Display in browser
- Others: Download prompt

### 10.4 File Icons

**Icon Mapping:**
```javascript
const getFileIcon = (fileName) => {
  const ext = fileName.split('.').pop().toLowerCase();
  if (['pdf'].includes(ext)) return '📄';
  if (['doc', 'docx', 'txt'].includes(ext)) return '📝';
  if (['jpg', 'jpeg', 'png', 'gif'].includes(ext)) return '🖼️';
  if (['xls', 'xlsx', 'csv'].includes(ext)) return '📊';
  if (['zip', 'rar', '7z'].includes(ext)) return '📦';
  return '📎';
};
```

### 10.5 File Deletion

1. User clicks delete (❌) button next to file
2. Confirmation dialog: "Are you sure you want to delete this file?"
3. If confirmed, DELETE request to `/api/tasks/{id}/files`
4. Server removes file path from task.files array
5. Server attempts to delete physical file (ignores error if not found)
6. TaskUpdated broadcast via SignalR
7. UI updates removing file from list

### 10.6 File Limitations (Current)

- ⚠️ **No size limit**: Could fill disk space
- ⚠️ **No type validation**: Could upload executables
- ⚠️ **No virus scanning**: Security risk
- ⚠️ **No compression**: Large files consume bandwidth
- ⚠️ **No chunked uploads**: Large files may timeout
- ⚠️ **No progress indicator**: User doesn't see upload progress

**Recommended Improvements:**
- Add file size limit (e.g., 10MB per file)
- Whitelist allowed file types
- Add virus scanning (ClamAV)
- Implement chunked uploads for large files
- Show upload progress bar
- Add thumbnail generation for images

---

## 11. ERROR HANDLING

### 11.1 Error Categories

**Client Errors (4xx):**
- 400 Bad Request: Invalid input, validation errors
- 401 Unauthorized: Missing/invalid JWT token
- 403 Forbidden: Share expired, max views reached
- 404 Not Found: Task/user not found
- 409 Conflict: Duplicate resource

**Server Errors (5xx):**
- 500 Internal Server Error: Unexpected server errors
- 503 Service Unavailable: Database connection issues

### 11.2 Error Response Format

```json
{
  "error": "ValidationError",
  "message": "ShortTitle is required",
  "field": "shortTitle",
  "timestamp": "2025-10-10T12:00:00Z"
}
```

### 11.3 Frontend Error Handling

**API Error Interceptor:**
```javascript
axios.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401) {
      // Logout user, redirect to login
      logout();
    }
    return Promise.reject(error);
  }
);
```

**Component Error Boundaries:**
```javascript
// Future: Add error boundaries to catch React errors
class ErrorBoundary extends React.Component {
  componentDidCatch(error, errorInfo) {
    // Log error, show fallback UI
  }
}
```

### 11.4 User-Facing Error Messages

**Validation Errors:**
- "Title is required"
- "Due date must be in the future"
- "Please select a status"

**Operation Errors:**
- "Failed to create task. Please try again."
- "Unable to upload file. Check file size."
- "Could not connect to server. Check your internet connection."

**Share Errors:**
- "This share link has expired"
- "Maximum views reached for this link"
- "Unable to generate share link"

### 11.5 Logging Strategy

**Backend Logging:**
```csharp
// Current: Console logging
Console.WriteLine($"Error: {ex.Message}");

// Future: Structured logging
_logger.LogError(ex, "Failed to create task for tenant {TenantId}", tenantId);
```

**Frontend Logging:**
```javascript
// Current: Console logging
console.error('API error:', error);

// Future: Error tracking service (Sentry, AppInsights)
Sentry.captureException(error);
```

---

## 12. TESTING REQUIREMENTS

### 12.1 Unit Testing

**Backend Tests (Future):**
- Controller tests with mocked services
- Service tests with mocked repository
- Model validation tests
- JWT token generation/validation tests

**Framework:** xUnit, Moq

**Example Test:**
```csharp
[Fact]
public async Task CreateTask_ValidInput_ReturnsCreatedTask()
{
    // Arrange
    var mockService = new Mock<ITaskService>();
    var controller = new TasksController(mockService.Object);
    
    // Act
    var result = await controller.Create(new TaskItem { ... });
    
    // Assert
    Assert.IsType<CreatedAtActionResult>(result);
}
```

**Frontend Tests (Future):**
- Component rendering tests
- User interaction tests
- API service tests
- Context reducer tests

**Framework:** Jest, React Testing Library

**Example Test:**
```javascript
test('renders task title', () => {
  render(<TaskItem task={mockTask} />);
  expect(screen.getByText('Test Task')).toBeInTheDocument();
});
```

### 12.2 Integration Testing

**API Tests:**
- Full request/response cycle
- Database integration
- Authentication flow
- SignalR connection

**Tools:** Postman collections, automated API tests

### 12.3 End-to-End Testing

**User Flows:**
- Login → Create Task → Edit Task → Delete Task
- Create Task → Add Comment → Delete Comment
- Upload File → Download File → Delete File
- Generate Share Link → View Public Task

**Tools:** Playwright, Cypress

### 12.4 Performance Testing

**Load Tests:**
- 100 concurrent users creating tasks
- 1000 tasks displayed in list
- SignalR message broadcast latency
- File upload/download throughput

**Tools:** k6, JMeter

### 12.5 Security Testing

**Tests:**
- SQL/NoSQL injection attempts
- XSS payload attempts
- JWT tampering
- CSRF token validation
- Tenant isolation (cross-tenant access attempts)

**Tools:** OWASP ZAP, Burp Suite

---

## 13. DEPLOYMENT SPECIFICATIONS

### 13.1 Development Environment

**Backend:**
```bash
cd workspacev1/api
dotnet run
# Runs on http://localhost:5175
```

**Frontend:**
```bash
cd workspacev1/client
npm start
# Runs on http://localhost:3000
```

**Database:**
```bash
mongod --dbpath C:\data\db
# MongoDB running on localhost:27017
```

### 13.2 Environment Variables

**Backend (appsettings.json):**
```json
{
  "DatabaseSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "TaskManagement",
    "TaskCollectionName": "tasks"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-here",
    "Issuer": "TaskFlowAPI",
    "Audience": "TaskFlowClient",
    "ExpirationHours": 24
  }
}
```

**Frontend (.env):**
```
REACT_APP_API_URL=http://localhost:5175
```

### 13.3 Production Deployment (Future)

**Backend:**
- Host: Azure App Service / AWS Elastic Beanstalk
- SSL: Configure HTTPS with Let's Encrypt
- Environment: Production appsettings
- Logging: Application Insights / CloudWatch

**Frontend:**
- Host: Azure Static Web Apps / Netlify / Vercel
- Build: `npm run build`
- Deployment: CI/CD pipeline (GitHub Actions)

**Database:**
- Host: MongoDB Atlas (cloud)
- Connection: Secure connection string with credentials
- Backup: Automated daily backups

**File Storage:**
- Migrate to Azure Blob Storage / AWS S3
- CDN: Azure CDN / CloudFront for global distribution

### 13.4 Monitoring & Alerting (Future)

**Application Monitoring:**
- Uptime monitoring (Pingdom, UptimeRobot)
- Performance monitoring (New Relic, Datadog)
- Error tracking (Sentry, Rollbar)

**Infrastructure Monitoring:**
- Server health (CPU, memory, disk)
- Database performance (query times, connections)
- Network traffic (bandwidth usage)

**Alerts:**
- API response time > 500ms
- Error rate > 5%
- Server CPU > 80%
- Disk space < 20%

---

## APPENDIX A: FUTURE ENHANCEMENTS

### Phase 1 (High Priority)
- [ ] User registration UI
- [ ] Search functionality
- [ ] User profile management
- [ ] Dashboard with statistics

### Phase 2 (Medium Priority)
- [ ] Subtasks/checklist
- [ ] Advanced filters
- [ ] Email notifications
- [ ] Calendar view

### Phase 3 (Low Priority)
- [ ] User roles/permissions
- [ ] Team management
- [ ] Export/import
- [ ] Mobile app

---

## APPENDIX B: KNOWN ISSUES

1. **React StrictMode Disabled**: Causes SignalR double-connection, disabled for now
2. **Debug Logs in Navbar**: Remove before production deployment
3. **No File Upload Validation**: Security and storage risk
4. **localStorage for Auth**: More secure httpOnly cookies recommended
5. **No Automated Tests**: Testing strategy not implemented yet

---

## APPENDIX C: GLOSSARY

| Term | Definition |
|------|------------|
| JWT | JSON Web Token - compact, URL-safe token for authentication |
| SignalR | Real-time web functionality library by Microsoft |
| Multi-tenancy | Architecture where multiple organizations share same system |
| Criticality | Priority level of task (Low/Medium/High) |
| Activity Log | Automatic audit trail of task changes |
| Public Share | Shareable link for external task viewing |
| Hub | SignalR server component for broadcasting messages |
| Tenant | Organization or company using the system |
| CRUD | Create, Read, Update, Delete operations |

---

**Document Version:** 1.0  
**Last Updated:** October 10, 2025  
**Next Review:** When implementing major features
