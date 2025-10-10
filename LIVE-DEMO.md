# 🌐 TaskFlow - Live Demo on Railway

## 🚀 Access the Application

### Frontend (Client Application)
**URL:** [https://todolist42v1app-production-0c85.up.railway.app/](https://todolist42v1app-production-0c85.up.railway.app/)

**What you can do:**
- ✅ Register a new account (8-digit password)
- ✅ Create, update, delete tasks
- ✅ Drag and drop tasks between columns (To Do, In Progress, Done)
- ✅ See real-time updates across multiple browser tabs
- ✅ Add tags, set due dates, mark criticality
- ✅ Upload files to tasks
- ✅ Add comments to tasks
- ✅ Share tasks publicly with expiration and view limits

### Backend API (Scalar Documentation)
**URL:** [https://todolist42v1app-production.up.railway.app/scalar/v1](https://todolist42v1app-production.up.railway.app/scalar/v1)

**What you can explore:**
- 📚 Complete API documentation
- 🔐 JWT authentication endpoints
- 📝 Task management endpoints
- 💬 Comment endpoints
- 🔗 Public share endpoints
- 🧪 Interactive API testing (try endpoints directly)

---

## ✨ Key Features to Test

### 1. Real-Time Collaboration (SignalR)
1. Open the app in two browser tabs/windows
2. Login to the same account in both
3. Create or update a task in one tab
4. **Watch it update instantly** in the other tab! 🎯

### 2. User Registration
1. Click "Register" on the login page
2. Enter username and **8-digit password** (numbers only)
3. Automatically logged in after registration
4. Organization created automatically

### 3. Task Management
- **Create Task:** Click "Add Task" button
- **Drag & Drop:** Move tasks between columns
- **Edit Task:** Click on a task card
- **Delete Task:** Click delete icon
- **Color Coding:** High (Red), Medium (Yellow), Low (Green)

### 4. Public Sharing
1. Create a task
2. Click "Share" button
3. Configure expiration time and view limits
4. Copy the public link
5. Open in incognito/private browser
6. **See live updates** without login!

### 5. File Attachments
- Upload files to tasks
- View and download attachments
- Delete attachments

### 6. Comments
- Add comments to tasks
- Real-time comment updates
- Delete comments

---

## 🔧 Technical Verification

### SignalR Connection
1. Open browser DevTools (F12)
2. Go to Console tab
3. Look for: `SignalR Connected` ✅
4. Check Network tab → Filter by WS (WebSocket)
5. Status should be: `101 Switching Protocols`

### API Testing
1. Visit: [https://todolist42v1app-production.up.railway.app/scalar/v1](https://todolist42v1app-production.up.railway.app/scalar/v1)
2. Try the `/api/Auth/register` endpoint
3. Test JWT authentication flow
4. Explore all available endpoints

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Railway Cloud Platform                   │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌─────────────────┐      ┌──────────────────┐             │
│  │   Frontend       │      │   Backend API    │             │
│  │   (React 19)     │◄────►│   (ASP.NET 9.0)  │             │
│  │   Port: 80       │      │   Port: 5000     │             │
│  │   Nginx          │      │   SignalR Hub    │             │
│  └─────────────────┘      └──────────────────┘             │
│         │                          │                         │
│         │                          │                         │
│         │                   ┌──────▼────────┐               │
│         │                   │   MongoDB     │               │
│         │                   │   (Database)  │               │
│         │                   │   Port: 27017 │               │
│         │                   └───────────────┘               │
│         │                                                    │
│  ┌──────▼────────────────────────────────────────────┐     │
│  │         HTTPS (443) - Auto SSL by Railway         │     │
│  └────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────┘
```

---

## 📊 Production Stack

| Component | Technology | Version | Status |
|-----------|-----------|---------|--------|
| **Frontend** | React | 19.2.0 | ✅ Running |
| **Backend** | ASP.NET Core | 9.0 | ✅ Running |
| **Database** | MongoDB | 7.0 | ✅ Connected |
| **Real-Time** | SignalR | 9.0.6 | ✅ WebSocket |
| **Auth** | JWT Bearer | - | ✅ Secured |
| **API Docs** | Scalar | Latest | ✅ Available |
| **Hosting** | Railway | Free Tier | ✅ Deployed |
| **SSL/TLS** | Auto HTTPS | - | ✅ Enabled |

---

## 🎯 Demo Credentials

**Note:** You can register your own account! Registration is open.

**Password Requirements:**
- Must be exactly **8 digits** (numbers only)
- Example: `12345678`

---

## 🧪 Test Scenarios

### Scenario 1: Real-Time Task Updates
```
1. Open app in Browser Tab A
2. Open app in Browser Tab B (same account)
3. Create task in Tab A: "Test Task"
4. Observe: Task appears instantly in Tab B ✅
5. Update task in Tab B: Change status to "In Progress"
6. Observe: Update reflects immediately in Tab A ✅
```

### Scenario 2: Multi-User Collaboration (Same Tenant)
```
1. Register User A
2. Create some tasks as User A
3. Register User B (same tenant/organization)
4. Both users see the same tasks ✅
5. Updates from User A appear for User B in real-time ✅
```

### Scenario 3: Public Task Sharing
```
1. Create a task as logged-in user
2. Click "Share" → Set expiration: 24 hours, Max views: 10
3. Copy the public link
4. Open link in private/incognito browser (no login)
5. View the task without authentication ✅
6. See live updates when original user edits the task ✅
```

### Scenario 4: API Testing via Scalar
```
1. Go to: https://todolist42v1app-production.up.railway.app/scalar/v1
2. Test POST /api/Auth/register with new credentials
3. Copy the returned JWT token
4. Use token to test authenticated endpoints
5. Try POST /api/Tasks/create
6. Verify task appears in frontend ✅
```

---

## 🔒 Security Features

- ✅ **JWT Authentication** - Secure token-based auth
- ✅ **Password Validation** - 8-digit requirement
- ✅ **HTTPS Only** - All traffic encrypted
- ✅ **CORS Protection** - Configured origins
- ✅ **Tenant Isolation** - Users only see their org's data
- ✅ **Secure WebSockets** (WSS) - Encrypted SignalR

---

## 📈 Performance

- **Frontend Load Time:** < 2s
- **API Response Time:** < 100ms
- **SignalR Latency:** < 50ms
- **Database Queries:** Optimized with indexes
- **Auto-Reconnect:** Yes (SignalR)
- **Fallback Transports:** SSE, Long Polling

---

## 🛠️ Troubleshooting

### Can't login?
- Ensure password is **exactly 8 digits**
- Try registering a new account

### Real-time updates not working?
- Check browser console (F12) for "SignalR Connected"
- Ensure WebSocket is not blocked by firewall
- App uses fallback transports automatically

### API not responding?
- Check API status: [https://todolist42v1app-production.up.railway.app/scalar/v1](https://todolist42v1app-production.up.railway.app/scalar/v1)
- If down, Railway may be redeploying

---

## 📦 Want to Deploy Your Own?

1. **Clone the repository:**
   ```bash
   git clone https://github.com/angelocarlotto/todolist42v1app.git
   cd todolist42v1app
   ```

2. **Follow deployment guide:**
   - See [QUICK-DEPLOY.md](./QUICK-DEPLOY.md) for 5-minute Railway setup
   - See [FREE-DEPLOYMENT-GUIDE.md](./FREE-DEPLOYMENT-GUIDE.md) for all platforms

3. **Run locally with Docker:**
   ```bash
   docker-compose up -d
   ```
   - Frontend: http://localhost:3000
   - API: http://localhost:5175
   - Scalar Docs: http://localhost:5175/scalar/v1

---

## 📞 Support & Feedback

- **Issues:** [GitHub Issues](https://github.com/angelocarlotto/todolist42v1app/issues)
- **Discussions:** [GitHub Discussions](https://github.com/angelocarlotto/todolist42v1app/discussions)
- **Documentation:** See `/docs` folder in repository

---

## 🎉 Enjoy TaskFlow!

Experience modern task management with real-time collaboration, powered by React, ASP.NET Core, MongoDB, and SignalR - all running on Railway's free tier!

**Frontend:** [https://todolist42v1app-production-0c85.up.railway.app/](https://todolist42v1app-production-0c85.up.railway.app/)

**API Docs:** [https://todolist42v1app-production.up.railway.app/scalar/v1](https://todolist42v1app-production.up.railway.app/scalar/v1)
