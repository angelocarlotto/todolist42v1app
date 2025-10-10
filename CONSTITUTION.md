# TaskFlow Application - Project Constitution

**Version:** 1.0  
**Last Updated:** October 10, 2025  
**Project Name:** TaskFlow - Collaborative Task Management System

---

## 1. PROJECT VISION & MISSION

### Vision
To create a modern, real-time collaborative task management application that enables teams to efficiently organize, track, and complete work with full transparency and seamless communication.

### Mission
Provide an intuitive, feature-rich task management platform with:
- Real-time collaboration via SignalR
- Multi-tenant architecture for organizational isolation
- Rich task metadata and relationships
- Activity tracking and commenting
- File attachment capabilities
- Public sharing for external collaboration

---

## 2. CORE PRINCIPLES

### 2.1 Technical Principles
1. **Real-time First**: All updates propagate instantly via SignalR to all connected users
2. **Multi-tenant Isolation**: Complete data separation between organizations
3. **API-Driven**: RESTful API design with clear separation of concerns
4. **Type Safety**: Strong typing on backend (C#) with clear data contracts
5. **Modern Stack**: Latest stable versions of ASP.NET Core and React
6. **Security First**: JWT authentication, tenant validation on all operations

### 2.2 User Experience Principles
1. **Immediate Feedback**: Users see updates without refreshing
2. **Minimal Clicks**: Common actions require minimal interaction
3. **Visual Clarity**: Clear status indicators, priority levels, and overdue warnings
4. **Contextual Information**: All relevant data visible without navigation
5. **Forgiveness**: Confirmation dialogs for destructive actions

### 2.3 Development Principles
1. **Component Reusability**: Build isolated, reusable React components
2. **Single Source of Truth**: SignalR broadcasts are the source for state updates
3. **Error Handling**: Graceful degradation and clear error messages
4. **Performance**: Efficient queries, minimal re-renders, optimized bundles
5. **Maintainability**: Clear code structure, meaningful names, documentation

---

## 3. ARCHITECTURAL DECISIONS

### 3.1 Technology Stack

**Backend:**
- **Framework**: ASP.NET Core 9.0 (Web API)
- **Database**: MongoDB (NoSQL for flexible schema)
- **Real-time**: SignalR for WebSocket communication
- **Authentication**: JWT tokens with claims-based identity
- **File Storage**: Physical file system (/uploads directory)

**Frontend:**
- **Framework**: React 19.2.0
- **State Management**: Context API with useReducer
- **Routing**: React Router v6
- **Real-time**: @microsoft/signalr client
- **Date Handling**: date-fns library
- **HTTP Client**: Axios

### 3.2 Architecture Patterns

**Backend Patterns:**
- Repository Pattern (TaskService)
- Controller-Service separation
- Dependency Injection
- Hub-based real-time communication
- Claims-based authorization

**Frontend Patterns:**
- Context-based global state
- Component composition
- Custom hooks for logic reuse
- Controlled components for forms
- Event-driven updates via SignalR

### 3.3 Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| MongoDB over SQL | Flexible schema for evolving task metadata, easier JSON handling |
| SignalR for real-time | Native .NET integration, automatic fallback mechanisms |
| JWT Authentication | Stateless, scalable, includes tenant/user claims |
| React Context API | Built-in, sufficient for app size, no external dependency |
| Physical file storage | Simple, cost-effective, easy to migrate to cloud later |
| Tenant in JWT claims | Security - prevents cross-tenant data access |

---

## 4. DATA MODEL

### 4.1 Core Entities

**TaskItem:**
```javascript
{
  id: ObjectId,
  shortTitle: string,
  description: string,
  status: enum["ToDo", "InProgress", "Done"],
  criticality: enum["Low", "Medium", "High"],
  dueDate: DateTime,
  tags: string[],
  files: string[], // File paths
  assignedUsers: string[], // User IDs
  
  // Multi-tenancy
  tenantId: string,
  userId: string, // Creator
  
  // Public sharing
  publicShareId: string?,
  shareExpiresAt: DateTime?,
  shareMaxViews: int?,
  shareViewCount: int,
  shareAllowEdit: boolean,
  
  // Collaboration
  comments: Comment[],
  activityLog: ActivityLog[],
  
  // Audit
  createdAt: DateTime,
  createdBy: string,
  updatedAt: DateTime,
  updatedBy: string,
  completedAt: DateTime?,
  completedBy: string?
}
```

**Comment:**
```javascript
{
  id: ObjectId,
  userId: string,
  username: string,
  text: string,
  createdAt: DateTime
}
```

**ActivityLog:**
```javascript
{
  id: ObjectId,
  userId: string,
  username: string,
  activityType: string, // "Created", "Updated", "StatusChanged", "Assigned", "Unassigned"
  description: string,
  oldValue: string?,
  newValue: string?,
  timestamp: DateTime
}
```

**User:**
```javascript
{
  id: ObjectId,
  username: string,
  passwordHash: string,
  tenantId: string,
  role: string,
  createdAt: DateTime
}
```

**Tenant:**
```javascript
{
  id: ObjectId,
  name: string,
  createdAt: DateTime
}
```

---

## 5. API CONTRACT

### 5.1 Authentication Endpoints
- `POST /api/auth/login` - User login, returns JWT token
- `POST /api/auth/register` - User registration (planned)
- `GET /api/auth/me` - Get current user info

### 5.2 Task Endpoints
- `GET /api/tasks` - List all tasks for tenant
- `GET /api/tasks/{id}` - Get specific task
- `POST /api/tasks` - Create new task
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Delete task
- `POST /api/tasks/{id}/assign` - Assign users to task
- `POST /api/tasks/{id}/unassign` - Unassign users from task
- `POST /api/tasks/{id}/upload` - Upload files to task
- `DELETE /api/tasks/{id}/files` - Delete file from task

### 5.3 Comment Endpoints
- `POST /api/comments/{taskId}` - Add comment to task
- `DELETE /api/comments/{taskId}/{commentId}` - Delete comment

### 5.4 Share Endpoints
- `POST /api/share/{taskId}` - Create public share link
- `DELETE /api/share/{taskId}` - Revoke public share
- `GET /api/public/task/{publicShareId}` - Get task via public link

### 5.5 Tenant Endpoints
- `GET /api/tenants/users` - List users in tenant
- `POST /api/tenants/users` - Add user to tenant

---

## 6. SIGNALR EVENTS

### 6.1 Server → Client Events

| Event | Payload | Description |
|-------|---------|-------------|
| `TaskCreated` | TaskItem | New task created |
| `TaskUpdated` | TaskItem | Task modified |
| `TaskDeleted` | string (taskId) | Task removed |
| `CommentAdded` | { taskId, commentId } | Comment added |
| `CommentDeleted` | { taskId, commentId } | Comment removed |
| `ReceiveReminder` | Reminder | Due date reminder |

### 6.2 Connection Groups
- **Tenant Group**: `{tenantId}` - All users in same organization
- **Public Group**: `public_{publicShareId}` - Public task viewers

---

## 7. SECURITY MODEL

### 7.1 Authentication
- JWT tokens with 24-hour expiration
- Tokens stored in localStorage (consider httpOnly cookies for production)
- Automatic token refresh on API calls

### 7.2 Authorization
- All API endpoints require authentication (except public share endpoints)
- Tenant ID validated from JWT claims on every request
- Users can only access data within their tenant
- Public share links bypass tenant restriction with limitations

### 7.3 Data Validation
- Input sanitization with HtmlEncode
- Required field validation
- Enum validation for status and criticality
- File upload size limits (future enhancement)

### 7.4 Security Considerations
- ⚠️ **TODO**: Implement rate limiting
- ⚠️ **TODO**: Add CORS configuration for production
- ⚠️ **TODO**: Implement refresh tokens
- ⚠️ **TODO**: Add file type validation for uploads
- ⚠️ **TODO**: Implement XSS protection in comments

---

## 8. DEPLOYMENT & INFRASTRUCTURE

### 8.1 Current Setup
- **Development**: 
  - Backend: `dotnet run` on http://localhost:5175
  - Frontend: `npm start` on http://localhost:3000
- **Database**: MongoDB connection string in appsettings.json
- **File Storage**: Local `/uploads` directory

### 8.2 Production Considerations (Future)
- [ ] Environment-specific configurations
- [ ] Cloud file storage (Azure Blob, AWS S3)
- [ ] Containerization (Docker)
- [ ] CI/CD pipeline
- [ ] Load balancing for SignalR
- [ ] MongoDB replica sets
- [ ] CDN for static assets
- [ ] HTTPS enforcement

---

## 9. QUALITY STANDARDS

### 9.1 Code Quality
- **Backend**: Follow C# coding conventions, XML documentation for public APIs
- **Frontend**: ESLint rules, component-level documentation
- **Testing**: Unit tests for business logic, integration tests for API endpoints (future)

### 9.2 Performance Targets
- API response time: < 200ms for CRUD operations
- Initial page load: < 2 seconds
- SignalR message latency: < 100ms
- Support for 100+ concurrent users per tenant

### 9.3 Browser Support
- Chrome (latest 2 versions)
- Firefox (latest 2 versions)
- Safari (latest 2 versions)
- Edge (latest 2 versions)

---

## 10. GOVERNANCE

### 10.1 Version Control
- Git-based version control
- Feature branches for new development
- Main branch always deployable

### 10.2 Change Management
- All API changes documented
- Database migrations tracked
- Breaking changes communicated in advance

### 10.3 Documentation Standards
- README.md for project overview
- CONSTITUTION.md for architectural decisions
- SPECIFICATIONS.md for detailed feature specs
- PLAN.md for development roadmap
- Inline code comments for complex logic

---

## 11. STAKEHOLDER ROLES

| Role | Responsibilities |
|------|-----------------|
| **Developer** | Implement features, fix bugs, maintain code quality |
| **Product Owner** | Define requirements, prioritize features |
| **Users** | Provide feedback, report issues, suggest improvements |
| **Administrator** | Manage deployments, monitor performance, ensure uptime |

---

## 12. SUCCESS METRICS

### 12.1 Technical Metrics
- ✅ Real-time updates working (< 100ms latency)
- ✅ Multi-tenant isolation enforced
- ✅ Zero cross-tenant data leaks
- ⏳ 99% uptime (production)
- ⏳ < 5% error rate

### 12.2 User Metrics
- ⏳ Daily active users
- ⏳ Tasks created per user per week
- ⏳ User retention rate
- ⏳ Feature adoption rates

### 12.3 Business Metrics
- ⏳ User satisfaction score
- ⏳ Time to complete tasks (avg)
- ⏳ Collaboration engagement (comments/task)

---

## 13. KNOWN LIMITATIONS & TECHNICAL DEBT

### Current Limitations:
1. No user registration UI (users must be created manually)
2. No password reset functionality
3. No email notifications
4. No search functionality
5. Limited error handling in frontend
6. No user roles/permissions
7. React StrictMode disabled (due to SignalR double-connection issue)
8. Debug logs in Navbar (remove for production)

### Technical Debt:
1. File uploads not validated (type, size)
2. No database indexes for performance
3. No caching layer
4. localStorage for auth tokens (security risk)
5. No automated tests
6. No API documentation (Swagger)
7. No logging/monitoring infrastructure

---

## 14. GLOSSARY

| Term | Definition |
|------|------------|
| **Tenant** | An organization or company using the system |
| **Multi-tenancy** | Multiple organizations using the same system with data isolation |
| **SignalR** | Microsoft's library for real-time web functionality |
| **JWT** | JSON Web Token - secure authentication token format |
| **Criticality** | Priority level of a task (Low/Medium/High) |
| **Public Share** | Shareable link allowing external users to view/edit tasks |
| **Activity Log** | Automatic audit trail of task changes |
| **Hub** | SignalR server component for broadcasting messages |

---

## 15. REVISION HISTORY

| Version | Date | Changes | Author |
|---------|------|---------|--------|
| 1.0 | Oct 10, 2025 | Initial constitution document | System |

---

**Document Status:** ✅ Current  
**Next Review Date:** When major architectural changes are proposed
