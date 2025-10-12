# 🐳 Docker Compose - How to Pass Build Arguments

## Your Question:
> "How to add args when executing docker compose?"

---

## 📚 Three Methods to Pass Build Args

### Method 1: Add `args` in docker-compose.yml (Recommended)

Update your `docker-compose.yml`:

```yaml
services:
  client:
    build:
      context: ./workspacev1/client
      dockerfile: Dockerfile
      args:                              # ← Add this section
        REACT_APP_API_URL: http://api:5175  # ← Build-time argument
    container_name: taskflow-client
    restart: unless-stopped
    ports:
      - "3000:80"
    # Note: environment is for RUNTIME, args is for BUILD TIME
    depends_on:
      - api
    networks:
      - taskflow-network
```

Then run:
```powershell
docker compose build
docker compose up -d
```

---

### Method 2: Use Environment Variables + docker-compose.yml

**Step 1**: Update `docker-compose.yml` to reference env vars:

```yaml
services:
  client:
    build:
      context: ./workspacev1/client
      dockerfile: Dockerfile
      args:
        REACT_APP_API_URL: ${REACT_APP_API_URL}  # ← Reads from .env or shell
    # ... rest of config
```

**Step 2**: Create `.env` file in same directory as `docker-compose.yml`:

```env
# .env file
REACT_APP_API_URL=http://localhost:5175
```

**Step 3**: Run compose:
```powershell
docker compose build
docker compose up -d
```

---

### Method 3: Pass Args via Command Line

Use `--build-arg` flag:

```powershell
# Build with custom arg
docker compose build --build-arg REACT_APP_API_URL=http://localhost:5175

# Then start containers
docker compose up -d
```

**Or combine both:**
```powershell
docker compose build `
  --build-arg REACT_APP_API_URL=http://localhost:5175 `
  --build-arg ANOTHER_VAR=value `
  client

docker compose up -d
```

---

## 🎯 Recommended Setup for Your Project

### Updated docker-compose.yml

```yaml
version: '3.8'

services:
  # MongoDB Database
  mongodb:
    image: mongo:7.0
    container_name: todoAppMongodb
    restart: unless-stopped
    environment:
      MONGO_INITDB_ROOT_USERNAME: admin
      MONGO_INITDB_ROOT_PASSWORD: taskflow2025
      MONGO_INITDB_DATABASE: TaskManagement
    ports:
      - "27018:27017"
    volumes:
      - mongodb_data:/data/db
      - mongodb_config:/data/configdb
    networks:
      - taskflow-network
    healthcheck:
      test: echo 'db.runCommand("ping").ok' | mongosh localhost:27017/test --quiet
      interval: 10s
      timeout: 5s
      retries: 5

  # Backend API
  api:
    build:
      context: ./workspacev1/api
      dockerfile: Dockerfile
    container_name: taskflow-api
    restart: unless-stopped
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:5175
      - DatabaseSettings__ConnectionString=mongodb://admin:taskflow2025@mongodb:27017
      - DatabaseSettings__DatabaseName=TaskManagement
      - DatabaseSettings__TaskCollectionName=tasks
      - JwtSettings__SecretKey=your-super-secret-key-change-in-production-minimum-32-characters
      - JwtSettings__Issuer=TaskFlowAPI
      - JwtSettings__Audience=TaskFlowClient
      - JwtSettings__ExpirationHours=24
    ports:
      - "5175:5175"
    volumes:
      - api_uploads:/app/uploads
    depends_on:
      mongodb:
        condition: service_healthy
    networks:
      - taskflow-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://127.0.0.1:5175/api/test"]
      interval: 30s
      timeout: 10s
      retries: 3

  # Frontend Client
  client:
    build:
      context: ./workspacev1/client
      dockerfile: Dockerfile
      args:
        # BUILD-TIME argument (baked into static files)
        REACT_APP_API_URL: ${REACT_APP_API_URL:-http://localhost:5175}
    container_name: taskflow-client
    restart: unless-stopped
    ports:
      - "3000:80"
    depends_on:
      - api
    networks:
      - taskflow-network
    healthcheck:
      test: ["CMD", "wget", "-q", "--spider", "http://127.0.0.1:80"]
      interval: 30s
      timeout: 10s
      retries: 3

volumes:
  mongodb_data:
    driver: local
  mongodb_config:
    driver: local
  api_uploads:
    driver: local

networks:
  taskflow-network:
    driver: bridge
```

### Create .env file

```env
# .env file in root directory (next to docker-compose.yml)

# For local development (client talks to api service via Docker network)
REACT_APP_API_URL=http://api:5175

# For production, you would use:
# REACT_APP_API_URL=https://todolist42v1app-api.up.railway.app
```

### Usage Commands

```powershell
# Development (uses .env file)
docker compose build
docker compose up -d

# Production (override with command line)
$env:REACT_APP_API_URL="https://todolist42v1app-api.up.railway.app"
docker compose build
docker compose up -d

# Or directly:
docker compose build --build-arg REACT_APP_API_URL=https://todolist42v1app-api.up.railway.app
docker compose up -d
```

---

## 🔍 Understanding the Difference

### `args` vs `environment` in docker-compose.yml

```yaml
services:
  client:
    build:
      context: ./workspacev1/client
      dockerfile: Dockerfile
      args:
        # ⏰ BUILD TIME - Used during "docker build"
        # Passed to Dockerfile ARG
        # CRA reads these and bakes into bundle
        REACT_APP_API_URL: http://api:5175
    
    environment:
      # ⏰ RUNTIME - Used when container runs
      # Available inside running container
      # NOT used by CRA (build already done)
      - NODE_ENV=production
```

### For Your React App:

```yaml
# ✅ CORRECT - Build-time arg for CRA
client:
  build:
    args:
      REACT_APP_API_URL: http://api:5175  # ← Used during npm run build

# ❌ WRONG - Runtime env (too late for CRA)
client:
  environment:
    - REACT_APP_API_URL=http://api:5175  # ← Not used by CRA!
```

---

## 📋 Complete Example with All Options

```yaml
services:
  client:
    build:
      context: ./workspacev1/client
      dockerfile: Dockerfile
      
      # BUILD ARGUMENTS (used during docker build)
      args:
        # Static values
        REACT_APP_API_URL: http://api:5175
        
        # From .env file
        REACT_APP_VERSION: ${APP_VERSION}
        
        # With default fallback
        REACT_APP_FEATURE_FLAG: ${FEATURE_FLAG:-false}
        
        # From shell environment
        BUILD_DATE: ${BUILD_DATE}
    
    # RUNTIME ENVIRONMENT (used when container runs)
    environment:
      - NODE_ENV=production
      - TZ=America/New_York
    
    # Or from .env file
    env_file:
      - .env.production
```

---

## 🧪 Testing Different Configurations

### Local Development:
```powershell
# .env file
REACT_APP_API_URL=http://api:5175

# Build and run
docker compose build
docker compose up -d

# Verify
docker exec taskflow-client sh -c "grep -r 'api:5175' /usr/share/nginx/html/static/js/"
```

### Production-like Testing:
```powershell
# Override via environment variable
$env:REACT_APP_API_URL="https://todolist42v1app-api.up.railway.app"
docker compose build
docker compose up -d

# Verify
docker exec taskflow-client sh -c "grep -r 'todolist42v1app-api' /usr/share/nginx/html/static/js/"
```

### Quick Test Without .env:
```powershell
# Direct command line
docker compose build --build-arg REACT_APP_API_URL=http://10.0.0.71:5175
docker compose up -d
```

---

## 🎯 Best Practice: Use .env Files

### Project Structure:
```
todolist42v1app/
├── docker-compose.yml
├── .env                      # ← Local development defaults
├── .env.production          # ← Production values
├── .env.staging            # ← Staging values
└── workspacev1/
    ├── client/
    │   ├── Dockerfile
    │   └── ...
    └── api/
```

### .env (Local Development)
```env
# Local Development
REACT_APP_API_URL=http://api:5175
APP_VERSION=1.0.0
```

### .env.production
```env
# Production
REACT_APP_API_URL=https://todolist42v1app-api.up.railway.app
APP_VERSION=1.0.0
```

### Usage:
```powershell
# Local
docker compose build
docker compose up -d

# Production
docker compose --env-file .env.production build
docker compose --env-file .env.production up -d
```

---

## 🔧 Troubleshooting

### Issue: Build arg not being used

**Check:**
```powershell
# See what args are being passed during build
docker compose build --progress=plain client
```

Look for:
```
#5 [build 2/8] ARG REACT_APP_API_URL
#5 DONE 0.0s

#6 [build 3/8] ENV REACT_APP_API_URL=http://api:5175
#6 DONE 0.0s
```

### Issue: Changes not taking effect

**Solution:**
```powershell
# Force rebuild (no cache)
docker compose build --no-cache client

# Or remove old images first
docker compose down
docker rmi taskflow-client
docker compose build
docker compose up -d
```

### Issue: Variable shows as empty

**Check Dockerfile has ARG:**
```dockerfile
ARG REACT_APP_API_URL  # ← Must be declared!
ENV REACT_APP_API_URL=${REACT_APP_API_URL}
```

**Check docker-compose.yml has args:**
```yaml
build:
  args:
    REACT_APP_API_URL: value  # ← Must be in args section!
```

---

## 📚 Docker Compose Reference

### Common Commands:

```powershell
# Build with args (from docker-compose.yml or .env)
docker compose build

# Build specific service
docker compose build client

# Build with no cache
docker compose build --no-cache

# Build with command-line arg override
docker compose build --build-arg KEY=value

# Use different .env file
docker compose --env-file .env.production build

# See build logs
docker compose build --progress=plain

# Build and start
docker compose up -d --build
```

---

## ✅ Your Updated docker-compose.yml

Replace your current client section with this:

```yaml
  # Frontend Client
  client:
    build:
      context: ./workspacev1/client
      dockerfile: Dockerfile
      args:
        # Build-time argument for CRA
        REACT_APP_API_URL: ${REACT_APP_API_URL:-http://localhost:5175}
    container_name: taskflow-client
    restart: unless-stopped
    ports:
      - "3000:80"
    depends_on:
      - api
    networks:
      - taskflow-network
    healthcheck:
      test: ["CMD", "wget", "-q", "--spider", "http://127.0.0.1:80"]
      interval: 30s
      timeout: 10s
      retries: 3
```

Then create `.env`:
```env
REACT_APP_API_URL=http://localhost:5175
```

And rebuild:
```powershell
docker compose build client
docker compose up -d
```

---

## 🎓 Summary

### To Pass Build Args in Docker Compose:

1. **Add `args` in `docker-compose.yml`:**
   ```yaml
   build:
     args:
       REACT_APP_API_URL: http://api:5175
   ```

2. **Use `.env` file:**
   ```yaml
   build:
     args:
       REACT_APP_API_URL: ${REACT_APP_API_URL}
   ```

3. **Override via command line:**
   ```powershell
   docker compose build --build-arg REACT_APP_API_URL=value
   ```

**Best approach**: Use `.env` file for flexibility! 🚀

---

*Last Updated: October 11, 2025*
