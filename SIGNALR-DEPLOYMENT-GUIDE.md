# SignalR Deployment Guide

## 🌐 Live Demo - SignalR Working on Railway!

TaskFlow successfully deployed with real-time SignalR on Railway:

- **🎨 Frontend:** [https://todolist42v1app-production-0c85.up.railway.app/](https://todolist42v1app-production-0c85.up.railway.app/)
- **🔌 API:** [https://todolist42v1app-production.up.railway.app/scalar/v1](https://todolist42v1app-production.up.railway.app/scalar/v1)

**SignalR Status:** ✅ Working perfectly!
- WebSocket connections established
- Real-time task updates across all clients
- Automatic fallback to Server-Sent Events/Long Polling if needed

---

## 📦 Repository

**GitHub:** [https://github.com/angelocarlotto/todolist42v1app](https://github.com/angelocarlotto/todolist42v1app)

```bash
# Clone the repository
git clone https://github.com/angelocarlotto/todolist42v1app.git
cd todolist42v1app
```

---

## �🚨 Common SignalR Deployment Issues

### Issue: SignalR Not Working in Production

**Symptoms:**
- SignalR works locally but fails when deployed
- Real-time updates don't work
- Browser console shows WebSocket connection errors
- CORS errors related to credentials

---

## 🔧 Solutions Implemented

### 1. **Frontend: Dynamic API URL Configuration**

**Problem:** Hardcoded `localhost:5175` URL doesn't work in production.

**Solution:** Use environment variables for dynamic configuration.

```javascript
// signalr.js - BEFORE (❌ Wrong)
const hubUrl = 'http://localhost:5175/hub/collaboration';

// signalr.js - AFTER (✅ Correct)
const apiUrl = process.env.REACT_APP_API_URL || 'http://localhost:5175';
const hubUrl = `${apiUrl}/hub/collaboration`;
```

**Railway Frontend Environment Variables:**
```bash
REACT_APP_API_URL=https://your-backend-api.railway.app
```

---

### 2. **Backend: CORS Configuration for SignalR**

**Problem:** `AllowAnyOrigin()` is incompatible with SignalR's `AllowCredentials()`.

**Why This Matters:**
- SignalR with authentication requires `AllowCredentials()` for WebSocket connections
- `AllowCredentials()` cannot be used with `AllowAnyOrigin()` for security reasons
- You must specify explicit origins or use `SetIsOriginAllowed()` with validation

**Solution:** Use specific origins with wildcard matching.

```csharp
// Program.cs - BEFORE (❌ Wrong - breaks SignalR auth)
policy.AllowAnyOrigin()
      .AllowAnyHeader()
      .AllowAnyMethod();

// Program.cs - AFTER (✅ Correct)
policy.WithOrigins(
          "http://localhost:3000",
          "http://localhost:5175"
      )
      .SetIsOriginAllowed(origin => 
      {
          // Allow localhost with any port
          if (origin.StartsWith("http://localhost:") || 
              origin.StartsWith("https://localhost:"))
              return true;
          
          // Allow Railway
          if (origin.EndsWith(".railway.app") || 
              origin.EndsWith(".up.railway.app"))
              return true;
          
          // Allow other platforms...
          return false;
      })
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials();  // ✅ Required for SignalR!
```

---

### 3. **Transport Fallback Configuration**

**Problem:** WebSockets might be blocked by firewalls, proxies, or load balancers.

**Solution:** Configure fallback transports (Server-Sent Events, Long Polling).

```javascript
// signalr.js
this.connection = new HubConnectionBuilder()
  .withUrl(hubUrl, {
    accessTokenFactory: () => token,
    skipNegotiation: false,  // Let SignalR negotiate best transport
    transport: undefined     // Use all transports (WebSockets, SSE, LongPolling)
  })
  .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
  .configureLogging(LogLevel.Information)
  .build();
```

**Transport Priority (automatic):**
1. **WebSockets** (preferred - lowest latency)
2. **Server-Sent Events** (SSE - fallback)
3. **Long Polling** (final fallback - highest latency)

---

## 🚀 Railway-Specific Configuration

### Railway WebSocket Support

Railway **fully supports WebSockets** by default. No special configuration needed!

### Environment Variables Required

**Backend Service:**
```bash
PORT=5000
ASPNETCORE_URLS=http://0.0.0.0:5000
ASPNETCORE_ENVIRONMENT=Production
DatabaseSettings__ConnectionString=mongodb://mongo:27017
DatabaseSettings__DatabaseName=TaskFlowDB
Jwt__Key=your-super-secret-jwt-key-min-32-chars
```

**Frontend Service:**
```bash
REACT_APP_API_URL=https://your-backend-api.railway.app
```

### Railway Domain Configuration

**Automatic:**
- Railway assigns: `https://your-service-name.up.railway.app`
- SSL/TLS automatically handled
- WebSockets work over HTTPS (wss://)

**Custom Domain:**
If you add a custom domain (e.g., `api.yourdomain.com`):

1. Add to Railway service settings
2. Update frontend environment variable:
   ```bash
   REACT_APP_API_URL=https://api.yourdomain.com
   ```
3. **Update backend CORS** in `Program.cs`:
   ```csharp
   .SetIsOriginAllowed(origin => 
   {
       // Add your custom domain
       if (origin == "https://yourdomain.com" || 
           origin == "https://www.yourdomain.com")
           return true;
       
       // ...existing checks
   })
   ```

---

## 🧪 Testing SignalR in Production

### 1. Browser Console Testing

Open browser DevTools (F12) and check:

```javascript
// Should see in Console:
SignalR Connected
Joined group: user123
Joined group: tenant456
```

**Common Error Messages:**

❌ **"Failed to start connection: Error: WebSocket failed to connect"**
- **Cause:** CORS issue or WebSocket blocked
- **Fix:** Check CORS configuration includes your frontend URL

❌ **"Access to XMLHttpRequest has been blocked by CORS policy"**
- **Cause:** Missing `AllowCredentials()` or origin not allowed
- **Fix:** Verify `SetIsOriginAllowed()` includes your domain

❌ **"Connection started successfully, but disconnects immediately"**
- **Cause:** JWT authentication failing
- **Fix:** Check token is being sent correctly via `accessTokenFactory`

### 2. Network Tab Analysis

In browser DevTools → Network tab:

1. Look for WebSocket connection (filter by WS)
2. Check status: Should be `101 Switching Protocols`
3. Verify frames are being sent/received

**Successful Connection:**
```
Request URL: wss://your-api.railway.app/hub/collaboration
Status Code: 101 Switching Protocols
Upgrade: websocket
Connection: Upgrade
```

### 3. Backend Logs

Check Railway backend logs for:

```bash
SignalR Connected
User user123 joined group
Task created/updated events
```

---

## 🔍 Debugging Checklist

### Frontend Issues

- [ ] `REACT_APP_API_URL` environment variable set correctly
- [ ] No hardcoded `localhost` URLs in code
- [ ] Browser console shows connection attempts
- [ ] JWT token present in localStorage (`authToken`)

**Test in browser console:**
```javascript
// Check environment variable
console.log(process.env.REACT_APP_API_URL);

// Check token
console.log(localStorage.getItem('authToken'));
```

### Backend Issues

- [ ] CORS includes your frontend domain
- [ ] `AllowCredentials()` is present
- [ ] SignalR hub is mapped: `app.MapHub<CollaborationHub>("/hub/collaboration")`
- [ ] JWT authentication configured for SignalR (query string token)
- [ ] Railway backend logs show no CORS errors

**Check Railway logs:**
```bash
# In Railway dashboard → Backend service → Deployments → View Logs
# Should NOT see:
❌ CORS policy error
❌ WebSocket upgrade failed
❌ Unauthorized 401 errors

# Should see:
✅ SignalR connection started
✅ User joined group
✅ Events broadcasting
```

### CORS Configuration

- [ ] `WithOrigins()` or `SetIsOriginAllowed()` used (not `AllowAnyOrigin()`)
- [ ] `AllowCredentials()` is present
- [ ] Frontend URL matches exactly (https vs http, with/without trailing slash)
- [ ] Wildcard domains configured for Railway subdomains

### WebSocket Connection

- [ ] Railway service is deployed and running
- [ ] HTTPS enabled (Railway does this automatically)
- [ ] No proxy/firewall blocking WebSockets
- [ ] Fallback transports enabled in client

---

## 📝 Quick Reference: SignalR Connection Flow

```
1. Frontend loads → Reads REACT_APP_API_URL
   ↓
2. User logs in → Receives JWT token → Stores in localStorage
   ↓
3. AppContext initializes SignalR
   ↓
4. SignalR connects to: ${REACT_APP_API_URL}/hub/collaboration
   ↓
5. Sends JWT via accessTokenFactory
   ↓
6. Backend validates JWT (OnMessageReceived in Program.cs)
   ↓
7. CORS check (SetIsOriginAllowed)
   ↓
8. WebSocket negotiation (or fallback to SSE/LongPolling)
   ↓
9. Connection established → User joins groups
   ↓
10. Real-time updates work!
```

---

## 🎯 Production Deployment Steps

### Step 1: Deploy Backend to Railway

```bash
# In Railway dashboard:
1. Create new service from GitHub repo
2. Select backend folder: workspacev1/api
3. Add environment variables (see above)
4. Deploy
5. Copy backend URL: https://your-backend.railway.app
```

### Step 2: Deploy Frontend to Railway

```bash
# In Railway dashboard:
1. Create new service from same GitHub repo
2. Select frontend folder: workspacev1/client
3. Add environment variable:
   REACT_APP_API_URL=https://your-backend.railway.app
4. Deploy
```

### Step 3: Verify SignalR Connection

1. Open frontend URL
2. Login
3. Open browser DevTools (F12)
4. Check Console for "SignalR Connected"
5. Check Network tab for WebSocket connection (WS filter)
6. Create a task → Should appear instantly without refresh

---

## 🛡️ Security Best Practices

### 1. JWT Secret Key

**Never use default key in production!**

```bash
# Generate secure key (PowerShell):
$bytes = New-Object byte[] 32
(New-Object System.Security.Cryptography.RNGCryptoServiceProvider).GetBytes($bytes)
[Convert]::ToBase64String($bytes)

# Add to Railway backend:
Jwt__Key=<your-generated-key>
```

### 2. CORS Origins

**Be specific in production:**

```csharp
// Development: Allow localhost with any port
if (app.Environment.IsDevelopment())
{
    policy.SetIsOriginAllowed(origin => 
        origin.StartsWith("http://localhost:"));
}

// Production: Explicit domains only
if (app.Environment.IsProduction())
{
    policy.WithOrigins(
        "https://your-frontend.railway.app",
        "https://yourdomain.com"
    );
}
```

### 3. SignalR Authorization

Already implemented in `CollaborationHub.cs`:

```csharp
[Authorize]
public class CollaborationHub : Hub
{
    // Only authenticated users can connect
}
```

---

## 📊 Performance Considerations

### WebSocket vs Fallback Transports

| Transport | Latency | Overhead | Battery | Use Case |
|-----------|---------|----------|---------|----------|
| **WebSockets** | Lowest (< 50ms) | Lowest | Best | Production (preferred) |
| **Server-Sent Events** | Medium (~100ms) | Medium | Good | WebSocket blocked |
| **Long Polling** | Highest (> 500ms) | Highest | Poor | Last resort fallback |

### Automatic Reconnection

Configured retry intervals:
```javascript
.withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
// Retry immediately, then after 2s, 5s, 10s, 30s
```

**User Experience:**
- Connection lost → Automatic retry
- User sees "Reconnecting..." notification (implement in UI)
- Connection restored → Resume real-time updates

---

## 🔗 Additional Resources

- **SignalR Documentation:** https://learn.microsoft.com/en-us/aspnet/core/signalr/
- **Railway WebSocket Support:** https://docs.railway.app/reference/websockets
- **CORS with Credentials:** https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS

---

## ✅ Final Checklist Before Deployment

- [ ] Frontend `signalr.js` uses `REACT_APP_API_URL`
- [ ] Backend CORS configured with `AllowCredentials()`
- [ ] Backend CORS allows your frontend domain
- [ ] Railway environment variables set correctly
- [ ] JWT secret key changed from default
- [ ] Test login and real-time task updates locally
- [ ] Deploy backend → Get URL
- [ ] Deploy frontend with backend URL
- [ ] Test in production browser (DevTools open)
- [ ] Verify WebSocket connection established
- [ ] Test real-time updates (create/update tasks)

---

## 🎉 Success Indicators

When everything works correctly:

✅ Browser console: `SignalR Connected`  
✅ Network tab: WebSocket status `101 Switching Protocols`  
✅ Create task → Appears instantly without refresh  
✅ Update task → Changes appear in real-time  
✅ Multiple browser tabs → Changes sync across all tabs  
✅ No CORS errors in console  
✅ No authentication errors (401)  

**You're ready for production!** 🚀
