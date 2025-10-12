# 🎓 Railway Build Args - Official Behavior

## Railway Automatically Passes Service Variables as Build Args

### Official Railway Documentation:

From Railway Docs (https://docs.railway.app/guides/dockerfiles):

> **"Service variables are available as build arguments during the Docker build process."**

This means:
- Any variable you set in Railway's "Variables" tab
- Is automatically passed as `--build-arg` to Docker
- IF your Dockerfile declares it with `ARG`

---

## How It Works

### Step 1: You Set a Variable in Railway UI
```
REACT_APP_API_URL = https://todolist42v1app-api.up.railway.app
```

### Step 2: Railway Automatically Runs
```bash
docker build \
  --build-arg REACT_APP_API_URL=https://todolist42v1app-api.up.railway.app \
  --build-arg PORT=3000 \
  # ... all other service variables ...
  -t my-app:latest \
  .
```

### Step 3: Your Dockerfile Receives It
```dockerfile
ARG REACT_APP_API_URL  # ← Railway injects the value here
ENV REACT_APP_API_URL=${REACT_APP_API_URL}
```

### Step 4: CRA Sees It During Build
```bash
# During npm run build, CRA reads process.env.REACT_APP_*
RUN npm run build
```

---

## 📋 Evidence: Your Dockerfile is Configured Correctly

Your current Dockerfile:

```dockerfile
# Build stage
FROM node:18-alpine AS build
WORKDIR /app

# 1) Accept build arg for CRA env
ARG REACT_APP_API_URL                    # ← Declares the build arg
ENV REACT_APP_API_URL=${REACT_APP_API_URL}  # ← Makes it available to npm

COPY package*.json ./
RUN npm ci --production=false
COPY . .

# 2) CRA will read REACT_APP_* here
RUN npm run build  # ← process.env.REACT_APP_API_URL is available!
```

**This is the CORRECT way to handle CRA + Railway + Docker!**

---

## 🔍 How to Verify Railway is Passing the Variable

After you trigger a redeploy, check the build logs in Railway:

### Look for These Lines:

```dockerfile
Step 4/15 : ARG REACT_APP_API_URL
 ---> Running in abc123def456
Step 5/15 : ENV REACT_APP_API_URL=${REACT_APP_API_URL}
 ---> Running in abc123def456
```

If Railway passed the variable correctly, you'll see it used (though the value might not be visible for security).

### You can also add a debug line temporarily:

```dockerfile
ARG REACT_APP_API_URL
ENV REACT_APP_API_URL=${REACT_APP_API_URL}

# Temporarily add this to see the value in logs:
RUN echo "API URL: $REACT_APP_API_URL"  # Debug line

COPY package*.json ./
```

Then check Railway logs to see:
```
API URL: https://todolist42v1app-api.up.railway.app
```

---

## 🚀 Why Your Current Setup Wasn't Working

The variable is set correctly in Railway, but the issue is:

1. ✅ Variable exists in Railway: `REACT_APP_API_URL`
2. ✅ Dockerfile declares it: `ARG REACT_APP_API_URL`
3. ❌ **Old build is still deployed** (built before you added the variable)

Railway doesn't automatically rebuild when you add variables. You must:
- Manually trigger redeploy
- OR push a new commit
- OR change another setting to trigger rebuild

---

## 🆚 Build-Time vs Runtime: The Confusion

### What People Think (WRONG):
```
Railway variables → Only available at runtime → Can't use with CRA
```

### What Actually Happens (CORRECT):
```
Railway variables → Available at BUILD time (as Docker ARG) → Perfect for CRA!
              AND
              └─→ Available at RUN time (as ENV) → For backend apps
```

---

## 📖 Railway Variable Types

Railway has different places to set variables, which can be confusing:

### 1. Service Variables (What You're Using)
- Set in: Service → Variables tab
- Available: Build time (as ARG) + Runtime (as ENV)
- ✅ This is what you need for CRA

### 2. Shared Variables
- Set in: Project → Shared Variables
- Available: Multiple services
- Also passed as build args

### 3. Railway-Provided Variables
- Automatically set: `PORT`, `RAILWAY_ENVIRONMENT`, etc.
- Always available

---

## 🎯 The Complete Flow

```
┌──────────────────────────────────────────────────────────┐
│  1. You Add Variable in Railway UI                      │
│     REACT_APP_API_URL = https://...api...               │
└────────────────┬─────────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────────┐
│  2. You Trigger Redeploy (Manual or Git Push)           │
└────────────────┬─────────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────────┐
│  3. Railway Starts Docker Build                          │
│     docker build --build-arg REACT_APP_API_URL=https... │
└────────────────┬─────────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────────┐
│  4. Dockerfile Receives ARG                              │
│     ARG REACT_APP_API_URL                                │
│     ENV REACT_APP_API_URL=${REACT_APP_API_URL}          │
└────────────────┬─────────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────────┐
│  5. npm run build Executes                               │
│     CRA reads: process.env.REACT_APP_API_URL             │
│     Value: https://todolist42v1app-api.up.railway.app   │
└────────────────┬─────────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────────┐
│  6. Webpack Replaces in Code                             │
│     Source: const url = process.env.REACT_APP_API_URL;  │
│     Output: const url = "https://...api...";             │
└────────────────┬─────────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────────┐
│  7. Bundle Contains Hardcoded URL                        │
│     main.abc123.js has the URL as a string               │
└────────────────┬─────────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────────┐
│  8. Nginx Serves Static Files                            │
│     Browser downloads bundle with correct URL            │
└──────────────────────────────────────────────────────────┘
```

---

## ✅ Confirmation: Your Setup is Standard

This is the **standard, recommended approach** for deploying React apps with Docker:

### Used by:
- ✅ Create React App official docs
- ✅ Railway documentation
- ✅ Vercel, Netlify (different implementation but same concept)
- ✅ Heroku buildpacks
- ✅ AWS Amplify

### Benefits:
- ✅ Simple - no runtime logic needed
- ✅ Fast - no JavaScript overhead
- ✅ Secure - only exposes what's in bundle
- ✅ Cacheable - bundle can be CDN-cached
- ✅ Portable - works anywhere Docker runs

---

## 🔧 If You Needed Runtime Configuration Instead

Only do this if you have a specific requirement (like same image, multiple environments):

```dockerfile
# Add entrypoint script
FROM nginx:alpine AS final
COPY --from=build /app/build /usr/share/nginx/html

# Create script that runs when container starts
RUN cat > /docker-entrypoint.sh << 'EOF'
#!/bin/sh
# Generate config at runtime
cat > /usr/share/nginx/html/env-config.js << EOL
window._env_ = {
  REACT_APP_API_URL: '${REACT_APP_API_URL:-http://localhost:5175}'
};
EOL
exec nginx -g "daemon off;"
EOF

RUN chmod +x /docker-entrypoint.sh
ENTRYPOINT ["/docker-entrypoint.sh"]
```

Then in your code:
```javascript
const API_BASE_URL = window._env_?.REACT_APP_API_URL || 'http://localhost:5175';
```

**But you don't need this!** Your build-time approach is better for most cases.

---

## 🎓 Summary

### Your Question:
> "CRA inject the variable content at the build time but the Railway make variable available only at runtime, how to deal with that?"

### The Answer:
**Railway makes variables available at BOTH build time AND runtime!**

- **Build time**: As Docker ARG (for CRA, frontend builds)
- **Runtime**: As container ENV (for backend, dynamic apps)

Your Dockerfile is correctly using the build-time approach. Just needs a redeploy!

---

## 📚 References

- Railway Docs: https://docs.railway.app/guides/dockerfiles
- Railway Variables: https://docs.railway.app/guides/variables
- CRA Env Vars: https://create-react-app.dev/docs/adding-custom-environment-variables/
- Docker ARG vs ENV: https://docs.docker.com/engine/reference/builder/#arg

---

**Your setup is correct. Just redeploy in Railway to apply the variable!** 🚀
