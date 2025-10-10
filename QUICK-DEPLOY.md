# 🚀 Quick Deploy - Railway.app (5 Minutes)

## 🌐 Live Demo

See TaskFlow running on Railway:

- **🎨 Frontend:** [https://todolist42v1app-production-0c85.up.railway.app/](https://todolist42v1app-production-0c85.up.railway.app/)
- **🔌 API Docs (Scalar):** [https://todolist42v1app-production.up.railway.app/scalar/v1](https://todolist42v1app-production.up.railway.app/scalar/v1)

> ✅ **Deployed successfully!** SignalR real-time updates working perfectly on Railway.

---

## 📦 Repository

**GitHub:** [https://github.com/angelocarlotto/todolist42v1app](https://github.com/angelocarlotto/todolist42v1app)

## Why Railway?
✅ **100% FREE** for small apps ($5 credit/month)  
✅ **All-in-one:** Frontend + Backend + MongoDB  
✅ **SignalR works perfectly**  
✅ **Auto HTTPS + Custom domains**  
✅ **Deploy from GitHub in clicks**

---

## 🏃 Deploy NOW (5 Steps)

### 1️⃣ Clone or Push to GitHub (1 min)

```bash
# Option A: Clone the repository
git clone https://github.com/angelocarlotto/todolist42v1app.git
cd todolist42v1app

# Option B: If you already have the code locally, push to your own repo
cd C:\Carlotto\todolistapp\todolist42v1app
git remote add origin https://github.com/YOUR_USERNAME/taskflow.git
git push -u origin master
```

### 2️⃣ Sign Up Railway (30 sec)

1. Go to **https://railway.app**
2. Click **"Login"** → **"Login with GitHub"**
3. Authorize Railway

### 3️⃣ Deploy MongoDB (1 min)

1. Click **"New Project"**
2. Click **"+ New"** → **"Database"** → **"MongoDB"**
3. Wait 30 seconds for provisioning
4. Click on **MongoDB service** → **"Variables"** tab
5. **Copy** the `MONGO_URL` value (looks like: `mongodb://mongo:...@...railway.app:6379`)

### 4️⃣ Deploy Backend API (2 min)

1. Click **"+ New"** → **"GitHub Repo"** → **Select your repo**
2. Railway detects multiple services, choose **"Deploy All"** or select `workspacev1/api`
3. Click on the **API service** → **"Variables"** tab
4. Click **"+ New Variable"** and add these:

```
ASPNETCORE_ENVIRONMENT = Production
ASPNETCORE_URLS = http://+:5000
PORT = 5000
DatabaseSettings__ConnectionString = [Paste MONGO_URL from step 3]
DatabaseSettings__DatabaseName = TaskManagement
DatabaseSettings__TaskCollectionName = tasks
JwtSettings__SecretKey = your-super-secret-32-character-key-change-this-now
JwtSettings__Issuer = TaskFlowAPI
JwtSettings__Audience = TaskFlowClient
JwtSettings__ExpirationHours = 24
```

5. Click **"Settings"** tab → **"Generate Domain"** under **Public Networking**
6. **Copy the URL** (e.g., `https://taskflow-api-production.up.railway.app`)

### 5️⃣ Deploy Frontend (1 min)

1. Click **"+ New"** → **"GitHub Repo"** → **Select your repo again**
2. Select `workspacev1/client` directory
3. Click on the **Client service** → **"Variables"** tab
4. Add:

```
REACT_APP_API_URL = [Paste backend URL from step 4]
```

5. Click **"Settings"** → **"Generate Domain"**
6. **Copy your frontend URL** (e.g., `https://taskflow-client.up.railway.app`)

---

## 🔧 Fix CORS (Required!)

### Update Backend Code

**File:** `workspacev1/api/Program.cs`

Find this section (around line 20-30):

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

**Replace with:**

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",  // Local development
            "https://taskflow-client.up.railway.app"  // Your Railway URL
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});
```

**Push changes:**

```bash
git add .
git commit -m "Update CORS for Railway deployment"
git push
```

Railway will **auto-deploy** in 2-3 minutes! 🎉

---

## ✅ Test Your Live App

1. **Open your frontend URL:** `https://taskflow-client.up.railway.app`
2. **Register a new user:**
   - Username: `testuser`
   - Password: `12345678`
3. **Create a task** and verify real-time updates work!

---

## 📊 Monitor Your App

### Check Logs
1. Go to **Railway Dashboard**
2. Click on **API service**
3. Click **"Deployments"** → **"View Logs"**

### Check Database
1. Click on **MongoDB service**
2. Click **"Data"** tab (requires Railway Pro)
   
   OR connect via MongoDB Compass:
   - Connection string: Your `MONGO_URL` from Variables

### Check Usage (Stay Free!)
1. Click on **Project Settings** (⚙️ icon)
2. View **"Usage"** tab
3. Should be **< $5/month** to stay free

---

## 🆓 Free Tier Limits

| Resource | Free Tier | Your Usage |
|----------|-----------|------------|
| **Credit** | $5/month | ~$3/month ✅ |
| **Storage** | 100GB | ~5GB ✅ |
| **RAM** | 512MB per service | 512MB ✅ |
| **Bandwidth** | Unlimited | Unlimited ✅ |

**You're well within free limits!** 🎊

---

## 🔐 Security Checklist

After deployment:

- [ ] Change `JwtSettings__SecretKey` to a random 32+ char string
- [ ] Update CORS with your actual Railway URLs
- [ ] Test registration and login on live site
- [ ] Verify SignalR real-time updates work
- [ ] Check MongoDB has data after creating tasks
- [ ] Set up custom domain (optional)

---

## 🐛 Troubleshooting

### ❌ "Network Error" when logging in

**Fix:** CORS not updated. Follow "Fix CORS" section above.

### ❌ Backend shows "Unhealthy"

**Fix:** 
1. Check environment variables are set correctly
2. Verify `ASPNETCORE_URLS=http://+:5000`
3. Check logs for error messages

### ❌ Frontend is blank

**Fix:**
1. Check `REACT_APP_API_URL` is set correctly
2. Make sure it starts with `https://` not `http://`
3. Rebuild: Railway dashboard → Client service → "Redeploy"

### ❌ "Cannot connect to MongoDB"

**Fix:**
1. Verify `DatabaseSettings__ConnectionString` is correct
2. Copy exact value from MongoDB service Variables tab
3. Restart backend service

---

## 💡 Pro Tips

1. **Enable Auto-Deploy**
   - Railway → Settings → Connect GitHub
   - Now every `git push` auto-deploys! 🚀

2. **Add Custom Domain**
   - Railway → Settings → Domains
   - Add your domain (e.g., taskflow.com)
   - Update DNS records as shown

3. **Monitor Uptime**
   - Use free service like UptimeRobot.com
   - Ping your app every 5 minutes
   - Get alerts if it goes down

4. **Backup Database**
   ```bash
   # Export from Railway MongoDB
   mongodump --uri="[Your MONGO_URL]" --out=backup
   ```

---

## 🎯 Alternative: Render.com (Also Free)

If Railway doesn't work for you:

### Deploy to Render (3 Steps)

1. **MongoDB:** Use MongoDB Atlas (free at mongodb.com/cloud/atlas)
2. **Backend:** Render.com → New Web Service → Connect GitHub → Select Docker
3. **Frontend:** Render.com → New Static Site → Build command: `npm run build`

**Limitation:** Services sleep after 15 min inactivity (30 sec cold start)

**Link:** https://render.com

---

## 📈 Upgrade Path (When You Need It)

**Stay FREE if:**
- < 100 users
- < 10,000 requests/day
- Cold starts OK

**Upgrade to Railway Pro ($5/mo) for:**
- $5 starting credit + $5 usage credit = $10 total
- No cold starts
- Better performance
- Custom domains
- Database backups

**Upgrade to Railway Team ($20/mo) for:**
- $20 usage credit
- Team collaboration
- Priority support
- Advanced monitoring

---

## 🎉 You're Live!

**Your TaskFlow app is now:**
- ✅ Deployed to production
- ✅ Accessible worldwide
- ✅ Running on HTTPS
- ✅ Backed by MongoDB
- ✅ Real-time updates working
- ✅ **100% FREE!**

**Share your app:**
```
🔗 https://taskflow-client.up.railway.app
```

---

## 📚 What's Next?

Now that you're deployed:

1. ✅ **Test thoroughly** on live site
2. ✅ **Share with users** and get feedback
3. ⬜ **Add features** from PLAN.md (Phase 1)
4. ⬜ **Set up monitoring** (UptimeRobot)
5. ⬜ **Add custom domain** (optional)
6. ⬜ **Implement email notifications** (Phase 2)

---

**Questions?** Check the full guide: `FREE-DEPLOYMENT-GUIDE.md`

**Need help?** Railway has great docs: https://docs.railway.app

---

**Deployment time:** ⏱️ **5 minutes**  
**Cost:** 💰 **$0/month**  
**Difficulty:** 🟢 **Easy**

**GO DEPLOY! 🚀**
