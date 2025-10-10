# 📚 Scalar API Documentation

## ✅ Scalar is Now Available in Production!

Scalar provides a beautiful, interactive API documentation interface for your TaskFlow API.

---

## 🌐 Accessing Scalar

### Local Development
```
http://localhost:5175/scalar/v1
```

### Production (Railway)
```
https://your-api-url.railway.app/scalar/v1
```

**Example:**
```
https://taskflow-api-production.up.railway.app/scalar/v1
```

---

## 🎨 Features

Scalar provides:
- ✅ **Interactive API documentation** - Test endpoints directly in the browser
- ✅ **Request/Response examples** - See what data to send and expect
- ✅ **Authentication testing** - Add JWT tokens and test protected endpoints
- ✅ **Code generation** - Get code snippets in multiple languages
- ✅ **Beautiful UI** - Clean, modern interface with purple theme
- ✅ **OpenAPI 3.0** - Standards-compliant API specification

---

## 🔐 Testing with Authentication

### Step 1: Get JWT Token

1. **Open Scalar:** `https://your-api-url/scalar/v1`
2. **Navigate to:** `POST /api/auth/login`
3. **Click "Try it"**
4. **Enter credentials:**
   ```json
   {
     "username": "your-username",
     "password": "12345678"
   }
   ```
5. **Click "Send"**
6. **Copy the token** from the response

### Step 2: Add Token to Requests

1. **Click "Auth" button** at the top
2. **Select "Bearer Token"**
3. **Paste your JWT token**
4. **All subsequent requests** will include the token

### Step 3: Test Protected Endpoints

Now you can test protected endpoints like:
- `GET /api/tasks` - Get all tasks
- `POST /api/tasks` - Create a task
- `PUT /api/tasks/{id}` - Update a task
- `DELETE /api/tasks/{id}` - Delete a task

---

## 📖 Available Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login
- `POST /api/auth/logout` - Logout
- `GET /api/auth/me` - Get current user info
- `POST /api/auth/change-password` - Change password

### Tasks
- `GET /api/tasks` - Get all tasks
- `GET /api/tasks/{id}` - Get task by ID
- `POST /api/tasks` - Create new task
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Delete task
- `POST /api/tasks/{id}/assign` - Assign users to task
- `POST /api/tasks/{id}/unassign` - Unassign users from task

### Files
- `POST /api/tasks/{id}/upload` - Upload files
- `DELETE /api/tasks/{id}/files` - Delete file
- `GET /uploads/{filename}` - Download file

### Comments
- `GET /api/tasks/{id}/comments` - Get comments
- `POST /api/tasks/{id}/comments` - Add comment
- `DELETE /api/tasks/{id}/comments/{commentId}` - Delete comment

### Sharing
- `POST /api/share/{id}/share` - Share task publicly
- `POST /api/share/{id}/revoke-share` - Revoke share

### Public Access
- `GET /public/task/{publicShareId}` - View public task
- `PUT /public/task/{publicShareId}` - Update public task status

### Tenants
- `GET /api/tenants` - Get all tenants
- `POST /api/tenants` - Create tenant

### Reminders
- `GET /api/tasks/reminders` - Get reminders
- `POST /api/tasks/send-reminders` - Send reminders

### Monitoring
- `GET /api/test` - Health check
- `GET /accessibility` - Accessibility info
- `GET /metrics` - Performance metrics
- `GET /weatherforecast` - Sample endpoint

---

## 🎯 Example Workflow

### 1. Register a User
```
POST /api/auth/register
{
  "username": "testuser",
  "password": "12345678"
}
```

### 2. Login
```
POST /api/auth/login
{
  "username": "testuser",
  "password": "12345678"
}
```
**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "userId": "...",
    "username": "testuser",
    "tenantId": "..."
  }
}
```

### 3. Create a Task (with token)
```
POST /api/tasks
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...

{
  "title": "My First Task",
  "description": "Task description",
  "status": "todo",
  "priority": "medium"
}
```

### 4. Get All Tasks
```
GET /api/tasks
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

---

## 🔒 Security Considerations

### ⚠️ Production Deployment

**Scalar is now public in production!** This means:
- ✅ Anyone can see your API documentation
- ✅ Anyone can try to test endpoints
- ❌ But they can't access data without valid JWT tokens

### Is This Safe?

**YES** ✅ for your app because:
1. All protected endpoints require authentication
2. JWT tokens are validated on every request
3. Multi-tenant isolation prevents data leakage
4. Documentation doesn't expose sensitive data

### Should You Restrict It?

**Optional:** If you want Scalar only in development:

```csharp
// Only show Scalar in Development
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
```

**Recommendation:** Keep it public! Benefits:
- ✅ Easy API testing
- ✅ Documentation for frontend developers
- ✅ Helpful for mobile app developers
- ✅ Shows your API is well-documented
- ✅ Makes integration easier

---

## 🎨 Customization

### Current Theme
```csharp
options.WithTheme(ScalarTheme.Purple)
```

### Available Themes
- `ScalarTheme.Purple` (Current)
- `ScalarTheme.Blue`
- `ScalarTheme.Green`
- `ScalarTheme.Orange`
- `ScalarTheme.Red`
- `ScalarTheme.Dark`
- `ScalarTheme.Light`

### To Change Theme

Edit `Program.cs`:
```csharp
app.MapScalarApiReference(options =>
{
    options
        .WithTitle("TaskFlow API")
        .WithTheme(ScalarTheme.Blue)  // Change here
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});
```

---

## 📱 Mobile App Integration

Scalar makes it easy for mobile developers to integrate with your API:

1. **Share the Scalar URL:** `https://your-api-url/scalar/v1`
2. **Developers can:**
   - See all available endpoints
   - View request/response formats
   - Test authentication flow
   - Get code examples
   - Download OpenAPI spec

---

## 🔧 Advanced Features

### Download OpenAPI Spec

Access the raw OpenAPI specification:
```
GET https://your-api-url/openapi/v1.json
```

This can be imported into:
- Postman
- Insomnia
- Swagger UI
- API testing tools

### Code Generation

Scalar provides code snippets for:
- C# (HttpClient)
- JavaScript (fetch)
- Python (requests)
- cURL
- And more...

Just click the code example button in Scalar!

---

## 🚀 Share with Your Team

### For Frontend Developers
```
API Documentation: https://your-api-url.railway.app/scalar/v1

Quick Start:
1. Register: POST /api/auth/register
2. Login: POST /api/auth/login
3. Use JWT token in Authorization header
4. All endpoints documented in Scalar!
```

### For Mobile Developers
```
API Base URL: https://your-api-url.railway.app
Documentation: https://your-api-url.railway.app/scalar/v1
OpenAPI Spec: https://your-api-url.railway.app/openapi/v1.json

Authentication: Bearer token in Authorization header
```

---

## 📊 Monitoring & Debugging

### Test Endpoints

Use Scalar to quickly test:
- ✅ Is the API online? → `GET /api/test`
- ✅ Performance metrics → `GET /metrics`
- ✅ Authentication working? → `POST /api/auth/login`
- ✅ CORS configured? → Test from browser

---

## 💡 Tips

1. **Use Scalar for API exploration** - Faster than Postman for quick tests
2. **Share the Scalar URL** - Makes onboarding new developers easy
3. **Test authentication flow** - Verify JWT tokens work correctly
4. **Check response schemas** - See exact data structures
5. **Generate code snippets** - Copy code for your frontend

---

## 🐛 Troubleshooting

### Scalar Not Loading

**Problem:** Can't access `/scalar/v1`  
**Solution:** 
- Check API is running
- Verify URL is correct
- Check browser console for errors

### Endpoints Not Showing

**Problem:** Some endpoints missing in Scalar  
**Solution:**
- Ensure controllers have `[ApiController]` attribute
- Check routes are defined correctly
- Verify OpenAPI is configured in `Program.cs`

### Authentication Not Working

**Problem:** Can't test protected endpoints  
**Solution:**
1. Login via `/api/auth/login`
2. Copy the token from response
3. Click "Auth" button in Scalar
4. Select "Bearer Token"
5. Paste token
6. Try protected endpoint again

---

## 📚 Documentation Links

- **Scalar Official Docs:** https://github.com/scalar/scalar
- **OpenAPI Specification:** https://swagger.io/specification/
- **ASP.NET Core OpenAPI:** https://learn.microsoft.com/aspnet/core/fundamentals/openapi

---

## ✅ Summary

- **Scalar URL:** `https://your-api-url/scalar/v1`
- **Available in:** All environments (dev, staging, production)
- **Theme:** Purple
- **Features:** Interactive docs, code generation, auth testing
- **Security:** Safe - all endpoints require authentication
- **Purpose:** Make API integration easy for developers

---

**Your API documentation is now live and accessible to everyone!** 🎉

**Share it with your team and make integration a breeze!** 📚
