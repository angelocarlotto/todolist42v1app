# 🔌 Railway Deployment - Port Configuration Guide

## Frontend Port Configuration

### 📦 What's in the Dockerfile

**Exposed Port:** `80`

```dockerfile
# Frontend listens on port 80 (standard HTTP)
EXPOSE 80

# Nginx serves on port 80
CMD ["nginx", "-g", "daemon off;"]
```

---

## 🚀 How Railway Handles Ports

### Automatic Port Detection

Railway **automatically detects and exposes port 80** from your Dockerfile.

**You don't need to configure anything!** ✅

### How It Works

```
User Request
    ↓
Railway Load Balancer (HTTPS 443)
    ↓
Your Container (HTTP 80)
    ↓
Nginx serving React app
```

**Railway handles:**
- ✅ HTTPS/SSL automatically (443 → 80)
- ✅ Domain routing
- ✅ Load balancing
- ✅ Health checks

---

## 🔧 Port Configuration Summary

### Frontend (React + Nginx)

| Environment | Internal Port | External Port | Protocol |
|-------------|---------------|---------------|----------|
| **Local Docker** | 80 | 3000 | HTTP |
| **Railway** | 80 | 443 | HTTPS |

**Local:**
```yaml
# docker-compose.yml
ports:
  - "3000:80"  # Maps local 3000 → container 80
```

**Railway:**
- Container runs on port **80**
- Railway automatically exposes as **HTTPS (443)**
- Access via: `https://your-app.railway.app`

### Backend (ASP.NET Core)

| Environment | Internal Port | External Port | Protocol |
|-------------|---------------|---------------|----------|
| **Local Docker** | 5175 | 5175 | HTTP |
| **Railway** | 5000 | 443 | HTTPS |

**Local:**
```yaml
# docker-compose.yml
ports:
  - "5175:5175"  # Direct mapping
```

**Railway:**
```env
ASPNETCORE_URLS=http://+:5000
PORT=5000
```
- Container runs on port **5000**
- Railway exposes as **HTTPS (443)**
- Access via: `https://your-api.railway.app`

---

## ⚙️ Environment Variables Needed

### Frontend - NO PORT CONFIGURATION NEEDED ✅

Railway automatically detects port 80 from Dockerfile.

**Only need:**
```env
REACT_APP_API_URL=https://your-api.railway.app
```

### Backend - PORT Variable Required

```env
PORT=5000
ASPNETCORE_URLS=http://+:5000
```

**Why both?**
- `PORT` - Railway expects this variable
- `ASPNETCORE_URLS` - ASP.NET Core needs this to bind

---

## 🐳 Docker Port Configuration

### Frontend Dockerfile (Current - Perfect!)

```dockerfile
# ✅ Correct - Port 80 is standard for web servers
EXPOSE 80
```

**Why port 80?**
- Standard HTTP port
- Nginx default
- Railway auto-detects it
- No configuration needed

### Backend Dockerfile (Current - Perfect!)

```dockerfile
# ✅ Correct - Port 5000 with environment variable
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
```

**Why port 5000?**
- Common ASP.NET Core port
- Easy to configure
- Railway expects PORT env var

---

## 🌐 Railway Deployment Ports

### What Railway Does Automatically

1. **Reads Dockerfile**
   - Finds `EXPOSE 80` (frontend)
   - Finds `EXPOSE 5000` (backend)

2. **Creates Load Balancer**
   - Listens on HTTPS (443)
   - Routes to your container port

3. **Provides Domain**
   - `https://your-app.railway.app`
   - SSL/TLS automatically configured

4. **Health Checks**
   - Pings your container port
   - Restarts if unhealthy

### Port Mapping Flow

```
┌─────────────────────────────────────────┐
│ User's Browser                          │
│ https://app.railway.app                 │
└─────────────┬───────────────────────────┘
              │ Port 443 (HTTPS)
              ↓
┌─────────────────────────────────────────┐
│ Railway Load Balancer                   │
│ - Handles SSL/TLS                       │
│ - Routes traffic                        │
└─────────────┬───────────────────────────┘
              │ Port 80 (HTTP)
              ↓
┌─────────────────────────────────────────┐
│ Your Docker Container                   │
│ Nginx listening on port 80              │
│ Serves React app                        │
└─────────────────────────────────────────┘
```

---

## ❌ Common Mistakes (You're NOT Making!)

### ❌ Wrong: Hardcoding Different Ports

```dockerfile
# DON'T DO THIS for Railway
EXPOSE 3000  # React dev port - won't work in production
```

### ✅ Correct: Use Standard Port 80

```dockerfile
# DO THIS - Standard HTTP port
EXPOSE 80  # Nginx default, Railway auto-detects
```

### ❌ Wrong: No PORT Environment Variable (Backend)

```env
# Missing PORT variable
ASPNETCORE_URLS=http://+:5000
```

### ✅ Correct: Include PORT Variable

```env
# Both variables needed
PORT=5000
ASPNETCORE_URLS=http://+:5000
```

---

## 🔍 Verify Port Configuration

### After Railway Deployment

#### Check Frontend

```bash
# Railway automatically assigns HTTPS
curl https://your-frontend.railway.app

# Should return: 200 OK with HTML
```

#### Check Backend

```bash
# Test API health endpoint
curl https://your-api.railway.app/api/test

# Should return: {"message":"API is working","timestamp":"..."}
```

#### Check Scalar Documentation

```bash
# API documentation should be accessible
curl https://your-api.railway.app/scalar/v1

# Should return: 200 OK with HTML
```

---

## 🛠️ Troubleshooting Ports

### Frontend Not Accessible

**Problem:** Can't access `https://your-app.railway.app`

**Solutions:**
1. Check Railway logs: Service → Deployments → View Logs
2. Verify Dockerfile exposes port 80:
   ```dockerfile
   EXPOSE 80
   ```
3. Check nginx is running:
   ```bash
   # In Railway terminal (if available)
   ps aux | grep nginx
   ```

### Backend Not Accessible

**Problem:** Can't access API

**Solutions:**
1. Verify PORT environment variable is set to `5000`
2. Check ASPNETCORE_URLS is set to `http://+:5000`
3. Check Railway logs for binding errors
4. Test health endpoint: `/api/test`

### Port Mismatch Errors

**Problem:** "Failed to bind to address"

**Solution:**
Backend must match:
```env
PORT=5000
ASPNETCORE_URLS=http://+:5000
```

---

## 📋 Deployment Checklist - Ports

### Frontend

- [x] Dockerfile exposes port 80 ✅
- [x] Nginx configured to listen on port 80 ✅
- [x] docker-compose maps 3000:80 (local) ✅
- [x] No PORT env var needed ✅

### Backend

- [x] Dockerfile exposes port 5000 ✅
- [x] `ASPNETCORE_URLS=http://+:5000` in Dockerfile ✅
- [x] `PORT=5000` set in Railway env vars ✅
- [x] `ASPNETCORE_URLS=http://+:5000` set in Railway env vars ✅

---

## 🎯 Railway-Specific Configuration

### Frontend Service Settings

**Railway Dashboard → Frontend Service:**

| Setting | Value | Notes |
|---------|-------|-------|
| **Port** | Auto-detected (80) | No manual config needed |
| **Environment Variables** | `REACT_APP_API_URL` | Only this needed |
| **Health Check** | Auto (port 80) | Railway checks automatically |

### Backend Service Settings

**Railway Dashboard → Backend Service:**

| Setting | Value | Notes |
|---------|-------|-------|
| **Port** | 5000 | Via PORT env var |
| **Environment Variables** | See below | Multiple needed |
| **Health Check** | Auto (port 5000) | Railway checks `/api/test` |

**Backend Environment Variables:**
```env
PORT=5000
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5000
DatabaseSettings__ConnectionString=mongodb://...
DatabaseSettings__DatabaseName=TaskManagement
JwtSettings__SecretKey=your-secret-key
JwtSettings__Issuer=TaskFlowAPI
JwtSettings__Audience=TaskFlowClient
JwtSettings__ExpirationHours=24
```

---

## 💡 Pro Tips

### 1. Use Standard Ports

✅ **Frontend:** Port 80 (standard HTTP)
- Nginx default
- Railway auto-detects
- No issues

✅ **Backend:** Port 5000 or 8080 (common app ports)
- Easy to remember
- Well-supported
- Documented everywhere

### 2. Railway PORT Variable

Railway injects `PORT` environment variable. Make sure your backend reads it:

```csharp
// ASP.NET Core automatically reads PORT
// Just set ASPNETCORE_URLS
builder.WebHost.UseUrls("http://+:5000");
```

### 3. Health Checks

Railway expects your app to respond on the configured port:

**Frontend (port 80):**
```
GET / → Returns index.html → Healthy ✅
```

**Backend (port 5000):**
```
GET /api/test → Returns JSON → Healthy ✅
```

### 4. Local vs Production

**Local (docker-compose):**
```yaml
frontend:
  ports:
    - "3000:80"    # External:Internal

backend:
  ports:
    - "5175:5000"  # External:Internal
```

**Railway:**
- Frontend: `https://app.railway.app` → Container port 80
- Backend: `https://api.railway.app` → Container port 5000
- Railway handles HTTPS automatically

---

## 🔐 Security Note

### Railway HTTPS Automatic

Railway automatically provides:
- ✅ SSL/TLS certificates (Let's Encrypt)
- ✅ HTTPS (443) to HTTP (80/5000) routing
- ✅ Certificate renewal
- ✅ Secure headers

**You don't need to:**
- ❌ Configure SSL in Nginx
- ❌ Set up certificates
- ❌ Handle HTTPS in your app
- ❌ Port 443 configuration

**Railway does it all!** 🎉

---

## 📊 Port Summary

| Component | Local | Railway Container | Railway External | Protocol |
|-----------|-------|-------------------|------------------|----------|
| **Frontend** | 3000 | 80 | 443 | HTTP→HTTPS |
| **Backend** | 5175 | 5000 | 443 | HTTP→HTTPS |
| **MongoDB** | 27018 | 27017 | N/A (private) | MongoDB |

---

## ✅ Your Current Configuration

### Perfect! ✅

Your Dockerfiles are already configured correctly:

**Frontend:**
```dockerfile
EXPOSE 80  ✅ Standard HTTP port
```

**Backend:**
```dockerfile
EXPOSE 5000  ✅ Common app port
ENV ASPNETCORE_URLS=http://+:5000  ✅ Bind to all interfaces
```

**No changes needed!** 🎉

---

## 🚀 Deployment Steps (Port-Related)

### Step 1: Deploy Frontend
1. Railway auto-detects port 80 from Dockerfile
2. No port configuration needed
3. Railway provides HTTPS URL

### Step 2: Deploy Backend
1. Set `PORT=5000` in Railway env vars
2. Set `ASPNETCORE_URLS=http://+:5000`
3. Railway provides HTTPS URL

### Step 3: Connect Frontend to Backend
1. Update frontend env: `REACT_APP_API_URL=https://your-api.railway.app`
2. Both services communicate via HTTPS
3. Done! ✅

---

## 📚 Related Documentation

- **QUICK-DEPLOY.md** - Complete deployment guide
- **RAILWAY-DEPLOYMENT.md** - Railway-specific setup
- **DOCKER-SUMMARY.md** - Docker configuration reference

---

**Summary:** Your frontend will be exposed on **HTTPS (443)** by Railway, served from internal **port 80**. No configuration changes needed! ✅
