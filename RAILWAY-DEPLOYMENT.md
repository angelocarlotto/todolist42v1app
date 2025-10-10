# 📝 Railway Deployment Configuration

## 🌐 Live Demo - Successfully Deployed!

TaskFlow is now live on Railway:

- **🎨 Frontend Application:** [https://todolist42v1app-production-0c85.up.railway.app/](https://todolist42v1app-production-0c85.up.railway.app/)
- **🔌 Backend API (Scalar Docs):** [https://todolist42v1app-production.up.railway.app/scalar/v1](https://todolist42v1app-production.up.railway.app/scalar/v1)

**Verified Working:** ✅
- Real-time SignalR updates
- JWT authentication & user registration
- MongoDB persistence
- CORS with credentials
- API documentation (Scalar)
- HTTPS with automatic SSL

---

## 📦 Repository

**GitHub:** [https://github.com/angelocarlotto/todolist42v1app](https://github.com/angelocarlotto/todolist42v1app)

```bash
# Clone the repository
git clone https://github.com/angelocarlotto/todolist42v1app.git
cd todolist42v1app
```

---

## ✅ CORS Configuration Added

The backend API (`Program.cs`) now includes CORS configuration for Railway deployment:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy(DefaultCorsPolicy, policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",                    // Local development
            "https://taskflow-client.railway.app"       // Railway frontend
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});
```

---

## 🚀 Ready to Deploy!

Your application is now configured for Railway deployment. Follow these steps:

### Step 1: Get the Code

The code is already on GitHub at: https://github.com/angelocarlotto/todolist42v1app

You can either:
- **Deploy directly** from this repository to Railway
- **Fork** the repository to your own GitHub account

### Step 2: Deploy to Railway

1. **Go to:** https://railway.app
2. **Sign in with GitHub**
3. **Create New Project** → **Deploy from GitHub repo**
4. **Select your repository**

### Step 3: Add Services

#### 3.1 MongoDB
- Click **"+ New"** → **"Database"** → **"MongoDB"**
- Copy the `MONGO_URL` from Variables tab

#### 3.2 Backend API
- Click **"+ New"** → **"GitHub Repo"** → Select repo
- **Root Directory:** `workspacev1/api`
- **Add Environment Variables:**
  ```
  ASPNETCORE_ENVIRONMENT=Production
  ASPNETCORE_URLS=http://+:5000
  PORT=5000
  DatabaseSettings__ConnectionString=[Paste MONGO_URL]
  DatabaseSettings__DatabaseName=TaskManagement
  DatabaseSettings__TaskCollectionName=tasks
  JwtSettings__SecretKey=[Generate 32+ random characters]
  JwtSettings__Issuer=TaskFlowAPI
  JwtSettings__Audience=TaskFlowClient
  JwtSettings__ExpirationHours=24
  ```
- **Generate Domain** (Settings → Public Networking)
- **Copy the API URL** (e.g., `https://taskflow-api-production.up.railway.app`)

#### 3.3 Frontend
- Click **"+ New"** → **"GitHub Repo"** → Select repo
- **Root Directory:** `workspacev1/client`
- **Add Environment Variable:**
  ```
  REACT_APP_API_URL=[Paste your API URL from step 3.2]
  ```
- **Generate Domain**
- **Copy the frontend URL**

### Step 4: Update CORS (If URL is Different)

If your Railway frontend URL is different from `https://taskflow-client.railway.app`:

1. Edit `workspacev1/api/Program.cs`
2. Update the CORS origin with your actual Railway URL
3. Commit and push:
   ```bash
   git add workspacev1/api/Program.cs
   git commit -m "Update CORS with actual Railway URL"
   git push
   ```
4. Railway will auto-deploy the update

---

## 🔐 Generate JWT Secret

Use PowerShell to generate a secure 32+ character secret:

```powershell
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | ForEach-Object {[char]$_})
```

Example output: `k9P2mNxQ8vW5rL7tH4fJ3bG6dC1aE0sZ`

---

## 📋 Environment Variables Checklist

### Backend API
- [ ] `ASPNETCORE_ENVIRONMENT=Production`
- [ ] `ASPNETCORE_URLS=http://+:5000`
- [ ] `PORT=5000`
- [ ] `DatabaseSettings__ConnectionString` (from MongoDB service)
- [ ] `DatabaseSettings__DatabaseName=TaskManagement`
- [ ] `DatabaseSettings__TaskCollectionName=tasks`
- [ ] `JwtSettings__SecretKey` (32+ chars)
- [ ] `JwtSettings__Issuer=TaskFlowAPI`
- [ ] `JwtSettings__Audience=TaskFlowClient`
- [ ] `JwtSettings__ExpirationHours=24`

### Frontend
- [ ] `REACT_APP_API_URL` (your Railway API URL)

---

## ✅ Testing Your Deployment

After deployment:

1. **Open your Railway frontend URL**
2. **Test registration:**
   - Username: `testuser`
   - Password: `12345678`
3. **Create a task**
4. **Verify real-time updates** (open in 2 browser tabs)
5. **Check SignalR connection** (browser console should show connection)

---

## 🐛 Troubleshooting

### ❌ CORS Error
**Problem:** Browser console shows CORS policy error  
**Solution:** 
- Verify the Railway frontend URL in `Program.cs` matches your actual URL
- Push changes and wait for auto-deploy (2-3 minutes)

### ❌ 502 Bad Gateway
**Problem:** Backend not responding  
**Solution:**
- Check environment variables are set correctly in Railway
- Verify `ASPNETCORE_URLS=http://+:5000` is set
- Check logs: Railway Dashboard → API Service → Deployments → View Logs

### ❌ MongoDB Connection Failed
**Problem:** Backend can't connect to database  
**Solution:**
- Copy exact `MONGO_URL` from MongoDB service Variables tab
- Paste into `DatabaseSettings__ConnectionString`
- Redeploy backend service

### ❌ SignalR Not Connecting
**Problem:** Real-time updates not working  
**Solution:**
- Verify CORS includes your frontend URL
- Check browser console for WebSocket errors
- Railway supports WebSockets by default, no extra config needed

---

## 📊 Monitor Your Deployment

### Check Logs
**Railway Dashboard → Service → Deployments → View Logs**

Common log messages:
- ✅ `Now listening on: http://[::]:5000` - Backend started
- ✅ `Application started` - All good
- ❌ `MongoDB connection failed` - Check connection string
- ❌ `CORS policy` - Update CORS origins

### Check Usage
**Railway Dashboard → Project Settings → Usage**

Your app should use:
- MongoDB: ~$1-2/month
- Backend: ~$1-2/month  
- Frontend: ~$0.50/month
- **Total: ~$3-4/month** ✅ (under $5 free credit)

---

## 🎉 Your App is Live!

Once deployed successfully:

- ✅ Frontend accessible worldwide
- ✅ Backend API running 24/7
- ✅ MongoDB database persistent
- ✅ HTTPS/SSL automatic
- ✅ Real-time SignalR working
- ✅ **100% FREE** (under $5/month credit)

**Share your app URL with users!**

---

## 🔄 Auto-Deploy Setup

Railway automatically deploys on every `git push` to GitHub:

1. Make code changes locally
2. Commit: `git commit -am "Your changes"`
3. Push: `git push`
4. Railway auto-deploys in 2-3 minutes
5. Check deployment status in Railway dashboard

---

## 📚 Additional Resources

- **Quick Deploy Guide:** `QUICK-DEPLOY.md`
- **Full Deployment Guide:** `FREE-DEPLOYMENT-GUIDE.md`
- **Platform Comparison:** `PLATFORM-COMPARISON.md`
- **Railway Docs:** https://docs.railway.app

---

## 🎯 Next Steps After Deployment

1. ✅ **Test all features** on production
2. ✅ **Share with users** and get feedback
3. ⬜ **Add custom domain** (optional, in Railway settings)
4. ⬜ **Set up monitoring** (UptimeRobot.com - free)
5. ⬜ **Implement Phase 1 features** (see PLAN.md)
6. ⬜ **Add email notifications** (Phase 2)

---

**Deployment Status:** 🟢 Ready to Deploy  
**Configuration:** ✅ Complete  
**CORS:** ✅ Configured  
**Documentation:** ✅ Available

**GO LIVE! 🚀**
