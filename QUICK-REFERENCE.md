# 📌 TaskFlow - Quick Reference Card

## 🌐 Live Demo Links

| Service | URL | Purpose |
|---------|-----|---------|
| **Frontend** | [todolist42v1app-production-0c85.up.railway.app](https://todolist42v1app-production-0c85.up.railway.app/) | Main Application |
| **API Docs** | [todolist42v1app-production.up.railway.app/scalar/v1](https://todolist42v1app-production.up.railway.app/scalar/v1) | Interactive API Documentation |
| **GitHub** | [github.com/angelocarlotto/todolist42v1app](https://github.com/angelocarlotto/todolist42v1app) | Source Code Repository |

---

## 🚀 Quick Start

### Try the Live Demo
1. Visit: [https://todolist42v1app-production-0c85.up.railway.app/](https://todolist42v1app-production-0c85.up.railway.app/)
2. Click "Register"
3. Enter username + 8-digit password
4. Start creating tasks!

### Test Real-Time Updates
1. Open app in 2 browser tabs
2. Login to same account
3. Create task in Tab 1
4. Watch it appear instantly in Tab 2 ✨

---

## 📚 Documentation Index

| Document | Description |
|----------|-------------|
| **README.md** | Main project overview with features |
| **LIVE-DEMO.md** | Complete demo guide with test scenarios |
| **QUICK-DEPLOY.md** | 5-minute Railway deployment |
| **FREE-DEPLOYMENT-GUIDE.md** | All free hosting platforms |
| **RAILWAY-DEPLOYMENT.md** | Railway-specific configuration |
| **SIGNALR-DEPLOYMENT-GUIDE.md** | SignalR production setup |
| **SIGNALR-FIX-SUMMARY.md** | SignalR troubleshooting |
| **PLATFORM-COMPARISON.md** | Compare hosting platforms |
| **RAILWAY-PORTS-GUIDE.md** | Port configuration guide |
| **SCALAR-DOCUMENTATION.md** | API documentation usage |
| **REGISTRATION-FEATURE.md** | User registration feature |

---

## 🛠️ Local Development

### Quick Start with Docker
```bash
# Clone repository
git clone https://github.com/angelocarlotto/todolist42v1app.git
cd todolist42v1app

# Start all services
docker-compose up -d

# Access services
# Frontend: http://localhost:3000
# API: http://localhost:5175
# Scalar: http://localhost:5175/scalar/v1
# MongoDB: localhost:27018
```

### Stop Services
```bash
docker-compose down
```

---

## 🎯 Key Features

| Feature | Status | Description |
|---------|--------|-------------|
| **Real-Time Updates** | ✅ | SignalR WebSocket synchronization |
| **User Authentication** | ✅ | JWT-based secure auth |
| **Task Management** | ✅ | CRUD operations with drag-drop |
| **Multi-Tenancy** | ✅ | Organization isolation |
| **Public Sharing** | ✅ | Time-limited public task links |
| **File Attachments** | ✅ | Upload/download task files |
| **Comments** | ✅ | Task discussion threads |
| **API Docs** | ✅ | Interactive Scalar documentation |
| **MongoDB** | ✅ | NoSQL database persistence |
| **Docker** | ✅ | Full containerization |

---

## 📊 Tech Stack

### Frontend
- **Framework:** React 19.2.0
- **UI:** Custom components
- **Real-Time:** @microsoft/signalr 9.0.6
- **HTTP Client:** Axios
- **Production Server:** Nginx

### Backend
- **Framework:** ASP.NET Core 9.0
- **Authentication:** JWT Bearer
- **Real-Time:** SignalR Hub
- **Database:** MongoDB Driver
- **API Docs:** Scalar + OpenAPI

### Infrastructure
- **Hosting:** Railway.app (Free Tier)
- **Database:** MongoDB 7.0
- **Containers:** Docker + Docker Compose
- **SSL/TLS:** Automatic HTTPS

---

## 🔐 Security

| Feature | Implementation |
|---------|----------------|
| **Authentication** | JWT Bearer tokens |
| **Password Policy** | 8-digit requirement |
| **HTTPS** | Automatic SSL (Railway) |
| **CORS** | Configured with credentials |
| **Tenant Isolation** | Data separated by organization |
| **Secure WebSockets** | WSS with authentication |

---

## 🧪 Testing Checklist

### ✅ Basic Functionality
- [ ] Register new account
- [ ] Login with credentials
- [ ] Create a task
- [ ] Update task details
- [ ] Delete task
- [ ] Drag task between columns

### ✅ Real-Time Features
- [ ] Open app in 2 tabs
- [ ] Create task in Tab 1
- [ ] See update in Tab 2
- [ ] Update task in Tab 2
- [ ] See update in Tab 1

### ✅ Advanced Features
- [ ] Upload file to task
- [ ] Add comment to task
- [ ] Share task publicly
- [ ] Access shared task without login
- [ ] See live updates on shared task

### ✅ API Testing
- [ ] Visit Scalar docs
- [ ] Test registration endpoint
- [ ] Test login endpoint
- [ ] Test task creation (with JWT)
- [ ] Verify task appears in frontend

---

## 🔧 Environment Variables

### Frontend (Railway)
```bash
REACT_APP_API_URL=https://todolist42v1app-production.up.railway.app
```

### Backend (Railway)
```bash
PORT=5000
ASPNETCORE_URLS=http://0.0.0.0:5000
ASPNETCORE_ENVIRONMENT=Production
DatabaseSettings__ConnectionString=<MONGO_URL>
DatabaseSettings__DatabaseName=TaskFlowDB
Jwt__Key=<YOUR_SECRET_KEY>
```

---

## 📞 Support

- **Issues:** [GitHub Issues](https://github.com/angelocarlotto/todolist42v1app/issues)
- **Documentation:** See repository `/docs`
- **Live Demo:** [Try it now!](https://todolist42v1app-production-0c85.up.railway.app/)

---

## 🎉 Quick Commands

```bash
# Clone and run locally
git clone https://github.com/angelocarlotto/todolist42v1app.git
cd todolist42v1app
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Rebuild after changes
docker-compose build
docker-compose up -d

# Push changes to GitHub
git add .
git commit -m "Your message"
git push origin master
```

---

## 🌟 Pro Tips

1. **Test Real-Time:** Open 2+ browser tabs to see SignalR in action
2. **Use Scalar:** Interactive API docs at `/scalar/v1`
3. **Check Console:** Press F12 to see SignalR connection status
4. **8-Digit Password:** Required for registration (e.g., `12345678`)
5. **Public Sharing:** Create limited-time task links for external viewers
6. **Railway Free Tier:** $5 credit/month - perfect for small apps

---

**Last Updated:** October 10, 2025  
**Version:** 1.0.0  
**Status:** ✅ Production Ready
