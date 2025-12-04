# Coolify Environment Variables Quick Fix

## Problem
The React app is making requests to `http://localhost:5175` instead of your production API.

## Root Cause
`REACT_APP_API_URL` is a **build-time** environment variable. It gets baked into the JavaScript bundle during `npm run build`. If it's not set correctly during the build, the wrong URL is hardcoded.

## Solution

### Step 1: Set Environment Variables in Coolify

Go to your Coolify project → Environment Variables and set:

```bash
# CRITICAL: This MUST be set as "Build and Runtime" (not just Runtime)
REACT_APP_API_URL=https://todo-api.angelocarlotto.com.br

# Also make sure these are set:
JWT_SECRET_KEY=<your-secret-from-openssl-rand-base64-32>
MONGO_CONNECTION_STRING=<your-mongodb-connection-string>
```

**Important:** The API URL should match your actual Coolify domain. Based on your logs, it might be:
- `https://todo_api.angelocarlotto.com.br` (with underscore)
- `https://todo-api.angelocarlotto.com.br` (with dash)
- Check your Coolify domains configuration to confirm

### Step 2: Variable Scope

In Coolify, when adding `REACT_APP_API_URL`:
1. Click "Add Variable"
2. Key: `REACT_APP_API_URL`
3. Value: `https://todo-api.angelocarlotto.com.br`
4. **Scope: Select "Build and Runtime"** (NOT "Runtime Only")
5. Save

### Step 3: Redeploy

Click "Deploy" in Coolify. This will:
1. Pull the latest code
2. Build the client with the correct `REACT_APP_API_URL`
3. The URL will be baked into the JavaScript bundle
4. Deploy the new version

### Step 4: Verify

After deployment:
1. Open browser dev tools (F12)
2. Go to Network tab
3. Try to login/register
4. Check if requests go to `https://todo-api.angelocarlotto.com.br` (not localhost)

## Why This Happens

Create React App (CRA) replaces `process.env.REACT_APP_API_URL` with the actual string value during build:

```javascript
// Before build (source code):
const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://10.0.0.71:5175';

// After build with REACT_APP_API_URL=https://api.prod.com:
const API_BASE_URL = 'https://api.prod.com';

// After build WITHOUT REACT_APP_API_URL:
const API_BASE_URL = 'http://10.0.0.71:5175'; // Uses fallback!
```

## Common Mistakes

❌ **Setting variable as "Runtime Only"** - Too late, bundle is already built
❌ **Not redeploying after setting variable** - Old bundle still has old URL
❌ **Wrong domain** - Make sure it matches your Coolify API service domain
❌ **Using internal service names** - Don't use `http://api:5175`, use public domain

## Check Current Build

To see what URL was baked in:
1. Deploy the app
2. Open the deployed app in browser
3. Open dev tools → Sources tab
4. Find the bundled JavaScript file
5. Search for your API URL - you'll see it hardcoded

## Alternative: Runtime Configuration

If you want to avoid rebuild for URL changes, you'd need to:
1. Load config from a JSON file at runtime
2. Or use a proxy/reverse proxy
3. Or inject config via index.html

But for now, just set `REACT_APP_API_URL` as build-time variable and redeploy.
