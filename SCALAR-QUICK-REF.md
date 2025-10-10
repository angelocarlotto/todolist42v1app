# 🚀 Quick Reference: Scalar API Documentation

## 📍 Access Scalar

**Local Development:**
```
http://localhost:5175/scalar/v1
```

**Production (Railway):**
```
https://your-api-url.railway.app/scalar/v1
```

---

## ⚡ Quick Start (1 Minute)

### Test Your First Endpoint

1. **Open Scalar** in browser
2. **Find** `POST /api/auth/login` endpoint
3. **Click** "Try it" button
4. **Enter:**
   ```json
   {
     "username": "testuser",
     "password": "12345678"
   }
   ```
5. **Click** "Send"
6. **Done!** See the response with JWT token

---

## 🔐 Test Protected Endpoints (2 Minutes)

### Step 1: Get Token
- Login via `POST /api/auth/login`
- Copy the `token` from response

### Step 2: Add Auth
- Click **"Auth"** button (top right)
- Select **"Bearer Token"**
- Paste your token
- Click **"Save"**

### Step 3: Test
- Try `GET /api/tasks`
- Try `POST /api/tasks`
- All protected endpoints now work! ✅

---

## 📋 Common Endpoints

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/auth/register` | POST | ❌ No | Register new user |
| `/api/auth/login` | POST | ❌ No | Login and get JWT |
| `/api/tasks` | GET | ✅ Yes | Get all tasks |
| `/api/tasks` | POST | ✅ Yes | Create new task |
| `/api/tasks/{id}` | PUT | ✅ Yes | Update task |
| `/api/tasks/{id}` | DELETE | ✅ Yes | Delete task |
| `/api/test` | GET | ❌ No | Health check |
| `/metrics` | GET | ❌ No | Performance metrics |

---

## 🎨 Features

✅ **Interactive Testing** - Test endpoints right in the browser  
✅ **Code Generation** - Get code snippets (C#, JavaScript, Python)  
✅ **Authentication** - Built-in JWT token management  
✅ **Beautiful UI** - Purple theme, clean design  
✅ **OpenAPI Spec** - Download full API specification  
✅ **Request History** - See previous requests  

---

## 💡 Pro Tips

1. **Use the search** - Type endpoint name to find it quickly
2. **Save tokens** - Auth persists in browser session
3. **Generate code** - Click code example for copy-paste snippets
4. **Download spec** - Get OpenAPI JSON for Postman/Insomnia
5. **Share URL** - Send Scalar link to team members

---

## 🔗 Share with Team

```
🚀 TaskFlow API Documentation

📖 Interactive Docs: https://your-api.railway.app/scalar/v1
🔌 Base URL: https://your-api.railway.app
📥 OpenAPI Spec: https://your-api.railway.app/openapi/v1.json

Quick Start:
1. Register: POST /api/auth/register
2. Login: POST /api/auth/login
3. Use JWT in Authorization: Bearer <token>
```

---

## 🐛 Troubleshooting

**Can't access Scalar:**
- Check API is running: `http://localhost:5175/api/test`
- Verify URL: `/scalar/v1` (not `/swagger`)

**Protected endpoints fail:**
- Did you add Bearer token in Auth?
- Is token valid? Try logging in again
- Check token format: `Bearer eyJhbGc...`

**No endpoints showing:**
- Refresh the page
- Check browser console for errors
- Verify OpenAPI is enabled

---

## 📚 Full Documentation

See **SCALAR-DOCUMENTATION.md** for:
- Complete endpoint list
- Authentication workflows
- Theme customization
- Advanced features
- Security considerations

---

**Status:** ✅ Live and working  
**Theme:** 🟣 Purple  
**Access:** 🌍 Public (secure with JWT)

**Enjoy your beautiful API documentation!** 🎉
