# 🚨 Railway Environment Variable Fix

## Problem Found

The production bundle contains the fallback URL `http://10.0.0.71:5175` instead of the Railway backend URL.

**Root Cause:** `REACT_APP_API_URL` environment variable is NOT set in Railway.

---

## ✅ Solution: Add Environment Variable

### Step 1: Go to Railway Dashboard

1. Visit: https://railway.app/dashboard
2. Select your **frontend project** (the React client)

### Step 2: Add Environment Variable

1. Click on **"Variables"** tab
2. Click **"New Variable"**
3. Add:
   ```
   Name:  REACT_APP_API_URL
   Value: https://todolist42v1app-api.up.railway.app
   ```
   ⚠️ **Important:** Use YOUR actual backend URL (without trailing slash)

### Step 3: Redeploy

After adding the variable, Railway will automatically redeploy your frontend.

**OR** manually trigger a redeploy:
1. Go to **"Deployments"** tab
2. Click **"Deploy"** on the latest deployment
3. Select **"Redeploy"**

---

## 🔍 Verification

After redeployment, verify the fix:

### Check 1: Environment Variable in Railway

In Railway dashboard → Variables tab, you should see:
```
REACT_APP_API_URL = https://todolist42v1app-api.up.railway.app
```

### Check 2: Build Logs

In Railway deployment logs, look for:
```
Creating an optimized production build...
```

The build should use the environment variable.

### Check 3: Test the Bundle (PowerShell)

```powershell
# Fetch the production bundle and check contents
$response = Invoke-WebRequest -Uri "https://todolist42v1app-production-0c85.up.railway.app/" -UseBasicParsing
$scriptUrl = [regex]::Match($response.Content, 'src="(/static/js/main\.[^"]+\.js)"').Groups[1].Value
$bundleUrl = "https://todolist42v1app-production-0c85.up.railway.app" + $scriptUrl
$bundle = (Invoke-WebRequest -Uri $bundleUrl -UseBasicParsing).Content

# Check what URL is in the bundle
if ($bundle -match 'todolist42v1app-production\.up\.railway\.app') {
    Write-Host "✅ SUCCESS: Railway URL found in bundle!" -ForegroundColor Green
} else {
    Write-Host "❌ FAILED: Railway URL NOT in bundle" -ForegroundColor Red
}
```

### Check 4: Test the Application

1. Visit: https://todolist42v1app-client.up.railway.app/
2. Open Browser DevTools (F12) → Network tab
3. Try to login or register
4. Check the API calls - they should go to:
   ```
   https://todolist42v1app-api.up.railway.app/api/auth/login
   ```
   NOT to `http://10.0.0.71:5175`

---

## 📝 Important Notes

### For Create React App (react-scripts):

1. ✅ Environment variables MUST start with `REACT_APP_`
2. ✅ They are replaced at BUILD TIME, not runtime
3. ✅ Must be set in Railway BEFORE the build runs
4. ✅ Requires redeploy to take effect (automatic after adding variable)

### Common Mistakes:

❌ **Setting variable AFTER build:** Won't work, needs redeploy
❌ **Wrong variable name:** Must be exactly `REACT_APP_API_URL`
❌ **Setting in backend project:** Must be set in FRONTEND project
❌ **Trailing slash:** Should NOT have trailing slash

---

## 🎯 Expected Result

After fixing:

### Before (Current - WRONG):
```javascript
// In production bundle:
const API_BASE_URL = "http://10.0.0.71:5175";
```

### After (Expected - CORRECT):
```javascript
// In production bundle:
const API_BASE_URL = "https://todolist42v1app-api.up.railway.app";
```

---

## 🆘 Troubleshooting

### Issue: Variable added but still not working

**Solution:** Make sure you're adding to the FRONTEND project, not backend
- Frontend URL: `https://todolist42v1app-client.up.railway.app`
- Backend URL: `https://todolist42v1app-api.up.railway.app`

### Issue: Build still using fallback

**Checklist:**
1. ✅ Variable name is exactly `REACT_APP_API_URL` (case-sensitive)
2. ✅ Value has no trailing slash
3. ✅ Value uses `https://` not `http://`
4. ✅ Redeploy was triggered after adding variable
5. ✅ Build logs show successful completion

### Issue: Can't find Variables tab

Railway UI changes sometimes:
- Look for **"Settings"** → **"Environment Variables"**
- Or **"Variables"** in left sidebar
- Or click on the service → **"Variables"** tab

---

## 📚 Reference

- Create React App Docs: https://create-react-app.dev/docs/adding-custom-environment-variables/
- Railway Docs: https://docs.railway.app/guides/variables

---

## ✅ Quick Command to Verify After Fix

```powershell
# Run this after Railway redeploys
Write-Host "Checking if environment variable was replaced..." -ForegroundColor Cyan
$response = Invoke-WebRequest -Uri "https://todolist42v1app-client.up.railway.app/" -UseBasicParsing
$scriptUrl = [regex]::Match($response.Content, 'src="(/static/js/main\.[^"]+\.js)"').Groups[1].Value
$bundle = (Invoke-WebRequest -Uri "https://todolist42v1app-client.up.railway.app$scriptUrl" -UseBasicParsing).Content
if ($bundle -match "todolist42v1app-api\.up\.railway\.app") {
    Write-Host "✅ FIXED!" -ForegroundColor Green
} else {
    Write-Host "❌ Still broken - check Railway variables" -ForegroundColor Red
}
```

---

**Next Steps:**
1. Go to Railway dashboard
2. Select frontend project
3. Add `REACT_APP_API_URL` variable
4. Wait for automatic redeploy
5. Test the application

🎉 After this fix, your application will correctly connect to the backend API!
