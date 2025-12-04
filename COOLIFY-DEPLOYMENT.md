# Coolify Deployment Guide

## Prerequisites
1. Coolify instance running
2. MongoDB Atlas or managed MongoDB instance (recommended for production)
3. Domain names configured in your DNS

## Required Environment Variables in Coolify

Set these in your Coolify project settings:

### MongoDB Configuration
```bash
MONGO_CONNECTION_STRING=mongodb+srv://username:password@cluster.mongodb.net/TaskManagement?retryWrites=true&w=majority
MONGO_DATABASE=TaskManagement
MONGO_COLLECTION=tasks
```

### API Configuration
```bash
API_DOMAIN=api.yourdomain.com
JWT_SECRET_KEY=<generate-with-openssl-rand-base64-32>
JWT_ISSUER=TaskFlowAPI
JWT_AUDIENCE=TaskFlowClient
JWT_EXPIRATION_HOURS=24
```

### Client Configuration
```bash
CLIENT_DOMAIN=app.yourdomain.com
REACT_APP_API_URL=https://api.yourdomain.com
```

## Deployment Steps

### Option 1: Use Coolify-Specific Compose File
1. In Coolify, create a new project
2. Connect your Git repository
3. Set Docker Compose path to: `.coolify/docker-compose.yml`
4. Add all environment variables listed above
5. Deploy

### Option 2: Use Main docker-compose.yml
1. In Coolify, create a new project
2. Connect your Git repository
3. Keep default docker-compose.yml path
4. Add environment variables
5. **Important**: If using external MongoDB, you don't need to deploy the mongodb service
   - Coolify will skip services with profiles by default

## Database Setup

### Option A: Use MongoDB Atlas (Recommended for Production)
1. Create a MongoDB Atlas cluster
2. Add your Coolify server IP to the IP whitelist
3. Create a database user
4. Get the connection string
5. Set `MONGO_CONNECTION_STRING` in Coolify

### Option B: Use Coolify's MongoDB Service
1. In Coolify, add a MongoDB service
2. Note the internal connection string
3. Set `MONGO_CONNECTION_STRING` to Coolify's internal MongoDB URL

## SSL/TLS
Coolify automatically handles SSL certificates via Let's Encrypt when you set domains in labels.

## Health Checks
The compose file includes health checks that Coolify will monitor:
- API: `GET /api/test`
- Client: HTTP check on port 80

## Troubleshooting

### Build fails
- Check build logs in Coolify
- Ensure Dockerfile paths are correct
- Verify all build args are set

### API can't connect to MongoDB
- Verify `MONGO_CONNECTION_STRING` is correct
- Check network connectivity
- Ensure MongoDB allows connections from Coolify server IP

### Client shows wrong API URL
- Remember: `REACT_APP_API_URL` is baked in at BUILD time
- Rebuild the client after changing this variable
- Use the full HTTPS URL: `https://api.yourdomain.com`

## Migration from Local to Coolify

1. Push your code to Git
2. Set up MongoDB Atlas or Coolify MongoDB
3. Configure environment variables in Coolify
4. Deploy using Coolify interface
5. Coolify will build and deploy both services
6. Access via your configured domains

## Important Notes

- The MongoDB service in docker-compose.yml has profiles and won't start in Coolify unless explicitly enabled
- Always use managed MongoDB for production (MongoDB Atlas, AWS DocumentDB, etc.)
- JWT_SECRET_KEY should be unique and secure (use `openssl rand -base64 32`)
- REACT_APP_API_URL must be the public URL of your API
- Coolify handles SSL/TLS automatically with Let's Encrypt
