# 🆓 Free Tier Deployment Guide for TaskFlow

**Last Updated:** October 10, 2025

This guide covers all the best **FREE** hosting options for your TaskFlow application (React + ASP.NET Core + MongoDB + SignalR).

---

## 🎯 Quick Comparison

| Platform | Frontend | Backend (.NET) | MongoDB | SignalR | Ease | Best For |
|----------|----------|----------------|---------|---------|------|----------|
| **Render** | ✅ Free | ✅ Free | ❌ (use Atlas) | ✅ Yes | ⭐⭐⭐⭐⭐ | **RECOMMENDED** |
| **Railway** | ✅ Free | ✅ Free | ✅ Free | ✅ Yes | ⭐⭐⭐⭐⭐ | **BEST OVERALL** |
| **Fly.io** | ✅ Free | ✅ Free | ✅ Free | ⚠️ WebSocket | ⭐⭐⭐⭐ | Tech-savvy users |
| **Azure** | ✅ Free | ✅ Free | ❌ (use Atlas) | ✅ Yes | ⭐⭐⭐ | Microsoft stack |
| **MongoDB Atlas** | N/A | N/A | ✅ Free | N/A | ⭐⭐⭐⭐⭐ | Database only |
| **Vercel** | ✅ Free | ❌ No .NET | N/A | ❌ | ⭐⭐⭐⭐ | Frontend only |
| **Netlify** | ✅ Free | ❌ No .NET | N/A | ❌ | ⭐⭐⭐⭐ | Frontend only |

---

## 🏆 RECOMMENDED: Railway.app (Best Free Option)

### ✅ Why Railway?
- **Completely FREE** tier: $5 credit/month (enough for small apps)
- **Supports all your stack**: React, .NET, MongoDB, Docker
- **SignalR works perfectly** (WebSocket support)
- **One-click deployment** from GitHub
- **Automatic HTTPS**
- **Great developer experience**

### 📦 What You Get FREE
- **Frontend:** Static site hosting
- **Backend:** .NET API with 512MB RAM
- **MongoDB:** Self-hosted with 1GB storage
- **Bandwidth:** 100GB/month
- **Build time:** Unlimited

### 🚀 Deployment Steps

#### Step 1: Prepare Your Repository

1. **Push your code to GitHub:**
   ```bash
   cd C:\Carlotto\todolistapp\todolist42v1app
   git init
   git add .
   git commit -m "Initial commit - TaskFlow app"
   git branch -M main
   git remote add origin https://github.com/YOUR_USERNAME/taskflow-app.git
   git push -u origin main
   ```

#### Step 2: Sign Up for Railway

1. Go to **https://railway.app**
2. Click **"Start a New Project"**
3. Sign in with **GitHub**
4. Authorize Railway to access your repositories

#### Step 3: Deploy MongoDB

1. Click **"+ New"** → **"Database"** → **"MongoDB"**
2. Railway will provision a MongoDB instance
3. Copy the **connection string** (click on MongoDB service → Variables → `MONGO_URL`)

#### Step 4: Deploy Backend API

1. Click **"+ New"** → **"GitHub Repo"** → Select your repository
2. Click **"Add Service"** → Select **"workspacev1/api"** folder
3. **Configure Environment Variables:**
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:5000
   DatabaseSettings__ConnectionString=[Paste MongoDB connection string]
   DatabaseSettings__DatabaseName=TaskManagement
   DatabaseSettings__TaskCollectionName=tasks
   JwtSettings__SecretKey=[Generate random 32+ character string]
   JwtSettings__Issuer=TaskFlowAPI
   JwtSettings__Audience=TaskFlowClient
   JwtSettings__ExpirationHours=24
   ```
4. Railway will auto-detect Dockerfile and deploy
5. Copy the **public URL** (e.g., `https://taskflow-api.railway.app`)

#### Step 5: Deploy Frontend

1. Click **"+ New"** → **"GitHub Repo"** → Select your repository again
2. Click **"Add Service"** → Select **"workspacev1/client"** folder
3. **Configure Environment Variables:**
   ```
   REACT_APP_API_URL=https://taskflow-api.railway.app
   ```
4. Railway will build and deploy
5. Your app is live! 🎉

#### Step 6: Configure CORS in Backend

Update `workspacev1/api/Program.cs` to allow your Railway frontend:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000", // Local development
            "https://taskflow-client.railway.app" // Railway frontend
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});
```

Push changes and Railway will auto-deploy!

### 💰 Cost Breakdown (FREE Tier)
- **Monthly credit:** $5 (automatically replenished)
- **Your usage:**
  - MongoDB: ~$1-2/month
  - Backend API: ~$1-2/month
  - Frontend: ~$0.50/month
- **Total:** ~$3.50/month ✅ **UNDER FREE LIMIT**

---

## 🥈 OPTION 2: Render.com (Solid Free Option)

### ✅ Why Render?
- **Generous free tier**
- **Automatic SSL/HTTPS**
- **Great for .NET apps**
- **Easy to use**

### 📦 What You Get FREE
- **Backend:** 750 hours/month (enough for 1 app 24/7)
- **Frontend:** Unlimited static site hosting
- **Limitation:** Services sleep after 15 min of inactivity (cold start)

### 🚀 Deployment Steps

#### Step 1: Deploy MongoDB (Use Atlas)

1. Go to **https://www.mongodb.com/cloud/atlas/register**
2. Create a **Free M0 cluster** (512MB storage)
3. Create database user and whitelist IP: **0.0.0.0/0** (allow all)
4. Get connection string

#### Step 2: Deploy Backend

1. Go to **https://render.com** → Sign up with GitHub
2. Click **"New +"** → **"Web Service"**
3. Connect your GitHub repo
4. **Configure:**
   - **Name:** taskflow-api
   - **Root Directory:** workspacev1/api
   - **Environment:** Docker
   - **Instance Type:** Free
5. **Environment Variables:**
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:5000
   DatabaseSettings__ConnectionString=[MongoDB Atlas connection string]
   DatabaseSettings__DatabaseName=TaskManagement
   JwtSettings__SecretKey=[Random 32+ chars]
   ```
6. Click **"Create Web Service"**
7. Copy URL (e.g., `https://taskflow-api.onrender.com`)

#### Step 3: Deploy Frontend

1. Click **"New +"** → **"Static Site"**
2. Connect your GitHub repo
3. **Configure:**
   - **Name:** taskflow-client
   - **Root Directory:** workspacev1/client
   - **Build Command:** `npm install && npm run build`
   - **Publish Directory:** build
4. **Environment Variables:**
   ```
   REACT_APP_API_URL=https://taskflow-api.onrender.com
   ```
5. Click **"Create Static Site"**

### ⚠️ Important: Cold Starts

Free tier services **sleep after 15 minutes** of inactivity. First request after sleep takes 30-60 seconds.

**Solution:** Use a cron service (like cron-job.org) to ping your API every 10 minutes.

---

## 🥉 OPTION 3: Fly.io (Docker-Friendly)

### ✅ Why Fly.io?
- **Excellent Docker support**
- **Global edge network**
- **Good free tier**

### 📦 What You Get FREE
- **3 VMs** with 256MB RAM each
- **3GB persistent storage**
- **160GB bandwidth/month**

### 🚀 Deployment Steps

#### Step 1: Install Fly CLI

```bash
# Windows (PowerShell)
pwsh -Command "iwr https://fly.io/install.ps1 -useb | iex"
```

#### Step 2: Login and Initialize

```bash
# Login
fly auth login

# Navigate to your project
cd C:\Carlotto\todolistapp\todolist42v1app

# Create MongoDB
fly postgres create --name taskflow-mongo --initial-cluster-size 1
```

#### Step 3: Deploy Backend

```bash
cd workspacev1/api

# Create fly.toml
fly launch --name taskflow-api --region ord --no-deploy

# Set environment variables
fly secrets set ASPNETCORE_ENVIRONMENT=Production
fly secrets set DatabaseSettings__ConnectionString=[MongoDB connection string]
fly secrets set JwtSettings__SecretKey=[Random string]

# Deploy
fly deploy
```

#### Step 4: Deploy Frontend

```bash
cd ../client

# Create fly.toml
fly launch --name taskflow-client --region ord --no-deploy

# Deploy
fly deploy
```

---

## 🔵 OPTION 4: Microsoft Azure (Free Tier)

### 📦 What You Get FREE
- **App Service:** 60 minutes/day (F1 tier)
- **Limitation:** Only 60 minutes of compute per day (not 24/7)

### 🚀 Quick Deploy

1. **Azure Portal:** https://portal.azure.com
2. Create **App Service** (F1 Free tier)
3. Deploy via **GitHub Actions** or **Docker**
4. Use **MongoDB Atlas** for database

**Note:** Not recommended for production due to 60-minute limit.

---

## 📊 FREE Database Options

### 🍃 MongoDB Atlas (RECOMMENDED)

**Free Tier (M0):**
- ✅ **512MB storage**
- ✅ **Shared RAM**
- ✅ **No credit card required**
- ✅ **24/7 uptime**
- ✅ **Automatic backups**

**Setup:**
1. Go to https://www.mongodb.com/cloud/atlas/register
2. Create free cluster (M0)
3. Create database user
4. Whitelist IP: `0.0.0.0/0`
5. Get connection string
6. Replace in your app

**Connection String Example:**
```
mongodb+srv://username:password@cluster0.xxxxx.mongodb.net/TaskManagement?retryWrites=true&w=majority
```

---

## 🎨 Frontend-Only Free Options

If you want to deploy frontend separately:

### Vercel (Best for React)
- **FREE:** Unlimited sites
- **Automatic HTTPS**
- **Deploy:** `npx vercel`
- **URL:** yourapp.vercel.app

### Netlify
- **FREE:** 100GB bandwidth/month
- **Automatic builds** from GitHub
- **URL:** yourapp.netlify.app

### GitHub Pages
- **FREE:** Unlimited
- **Static only**
- **URL:** username.github.io/repo

---

## 🏗️ Recommended Architecture for FREE Tier

```
┌─────────────────────────────────────────────────────┐
│  Frontend (React)                                   │
│  Hosted on: Railway or Netlify                     │
│  Cost: FREE                                         │
└─────────────┬───────────────────────────────────────┘
              │
              │ HTTPS API Calls
              ↓
┌─────────────────────────────────────────────────────┐
│  Backend API (.NET + SignalR)                       │
│  Hosted on: Railway or Render                      │
│  Cost: FREE                                         │
└─────────────┬───────────────────────────────────────┘
              │
              │ MongoDB Connection
              ↓
┌─────────────────────────────────────────────────────┐
│  MongoDB Database                                   │
│  Hosted on: MongoDB Atlas                           │
│  Cost: FREE (M0 cluster)                            │
└─────────────────────────────────────────────────────┘
```

---

## 📝 Pre-Deployment Checklist

Before deploying to any platform:

### 1. Update CORS Settings

**File:** `workspacev1/api/Program.cs`

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
            "https://your-frontend-url.com"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});
```

### 2. Update API URL in Frontend

**File:** Create `.env.production` in `workspacev1/client/`

```env
REACT_APP_API_URL=https://your-backend-api-url.com
```

### 3. Generate Strong JWT Secret

```bash
# PowerShell
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | ForEach-Object {[char]$_})
```

### 4. Environment Variables Needed

**Backend:**
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5000
DatabaseSettings__ConnectionString=mongodb+srv://...
DatabaseSettings__DatabaseName=TaskManagement
JwtSettings__SecretKey=your-32-char-secret
JwtSettings__Issuer=TaskFlowAPI
JwtSettings__Audience=TaskFlowClient
JwtSettings__ExpirationHours=24
```

**Frontend:**
```
REACT_APP_API_URL=https://your-api-url.com
```

### 5. Update docker-compose for Production

Already done! Your `docker-compose.prod.yml` is ready.

---

## 🚀 My Recommended Free Setup

### Best Option: Railway.app (All-in-One)

**Why?**
- ✅ Everything in one place
- ✅ $5 free credit/month (enough for your app)
- ✅ Full Docker support
- ✅ SignalR works perfectly
- ✅ Automatic HTTPS
- ✅ Easy deployment from GitHub
- ✅ Great developer experience

**Total Cost:** **$0/month** (stays under $5 credit limit)

### Alternative: Hybrid Approach

- **Frontend:** Vercel (FREE unlimited)
- **Backend:** Render.com (FREE 750 hours)
- **Database:** MongoDB Atlas (FREE M0)

**Total Cost:** **$0/month** (with cold starts on Render)

---

## 📋 Step-by-Step: Deploy to Railway (EASIEST)

### Complete Tutorial

```bash
# Step 1: Push to GitHub
git init
git add .
git commit -m "Deploy TaskFlow"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/taskflow.git
git push -u origin main

# Step 2: Go to railway.app and sign in with GitHub

# Step 3: Create new project
# - Click "New Project"
# - Select "Deploy from GitHub repo"
# - Choose your repository

# Step 4: Add MongoDB
# - Click "+ New"
# - Select "Database" → "MongoDB"
# - Copy connection string from Variables tab

# Step 5: Add Backend Service
# - Click "+ New" → "GitHub Repo"
# - Select root directory: workspacev1/api
# - Add environment variables (see checklist above)
# - Deploy!

# Step 6: Add Frontend Service
# - Click "+ New" → "GitHub Repo"
# - Select root directory: workspacev1/client
# - Add REACT_APP_API_URL=[backend URL from step 5]
# - Deploy!

# Step 7: Update CORS in code
# - Edit Program.cs with frontend URL
# - Push to GitHub
# - Railway auto-deploys!
```

**Time to deploy:** 15-20 minutes ⏱️

---

## 🆘 Troubleshooting

### Issue: CORS Error

**Solution:** Update `Program.cs` with your frontend URL and redeploy.

### Issue: MongoDB Connection Failed

**Solution:** 
- Check connection string format
- Whitelist IP `0.0.0.0/0` in MongoDB Atlas
- Verify username/password are correct

### Issue: SignalR Not Working

**Solution:**
- Ensure WebSocket support is enabled on platform
- Railway/Render: ✅ Works automatically
- Vercel/Netlify: ❌ Don't support WebSocket (use for frontend only)

### Issue: 502 Bad Gateway

**Solution:**
- Check backend logs
- Verify environment variables are set
- Make sure port 5000 is exposed (ASPNETCORE_URLS)

---

## 💡 Tips for Free Tier

1. **Use MongoDB Atlas** - Best free database option
2. **Deploy to Railway** - Easiest all-in-one solution
3. **Use GitHub Actions** - Automate deployments
4. **Monitor usage** - Stay under free limits
5. **Consider cold starts** - Render/Fly.io sleep after inactivity
6. **Use CDN** - Serve static files faster
7. **Optimize images** - Reduce bandwidth usage

---

## 📈 When to Upgrade?

Stay on **FREE tier** if:
- < 100 users
- < 10,000 requests/day
- < 1GB database
- Cold starts are acceptable

**Upgrade** ($5-20/month) when:
- Need 24/7 uptime (no cold starts)
- 100+ concurrent users
- > 1GB database
- Need custom domain
- Need better performance

---

## 🎓 Learning Resources

- **Railway Docs:** https://docs.railway.app
- **Render Docs:** https://render.com/docs
- **MongoDB Atlas Tutorial:** https://www.mongodb.com/basics/get-started
- **Fly.io Guide:** https://fly.io/docs/

---

## ✅ Next Steps

1. **Choose a platform** (I recommend Railway)
2. **Push code to GitHub**
3. **Sign up for free account**
4. **Follow deployment steps above**
5. **Test your live app!**

**Need help?** Check the troubleshooting section or deployment logs on your chosen platform.

---

**Your app can be live for FREE in under 20 minutes!** 🚀
