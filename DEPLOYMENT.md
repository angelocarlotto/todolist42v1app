# TaskFlow - Deployment Guide

This guide covers deploying TaskFlow to various environments using Docker.

## Table of Contents
1. [Local Development with Docker](#local-development-with-docker)
2. [Production Deployment](#production-deployment)
3. [Cloud Platform Deployment](#cloud-platform-deployment)
4. [Kubernetes Deployment](#kubernetes-deployment)
5. [Monitoring & Maintenance](#monitoring--maintenance)

---

## Local Development with Docker

### Quick Start

1. **Start all services:**
```bash
cd c:\Carlotto\todolistapp\todolist42v1app
docker-compose up -d
```

2. **View logs:**
```bash
docker-compose logs -f
```

3. **Access the application:**
- Frontend: http://localhost:3000
- Backend API: http://localhost:5175
- MongoDB: mongodb://admin:taskflow2025@localhost:27017

4. **Stop services:**
```bash
docker-compose down
```

### Useful Commands

```bash
# Rebuild and start
docker-compose up -d --build

# View specific service logs
docker-compose logs -f api
docker-compose logs -f client

# Execute commands in containers
docker-compose exec api bash
docker-compose exec mongodb mongosh -u admin -p taskflow2025

# Check status
docker-compose ps

# Restart a service
docker-compose restart api

# Clean up everything (including volumes)
docker-compose down -v
```

---

## Production Deployment

### Prerequisites
- Docker and Docker Compose installed on server
- Domain name configured (optional but recommended)
- SSL certificates (for HTTPS)

### Step 1: Prepare Environment Variables

Create `.env` file:

```bash
cp .env.example .env
```

Edit `.env` with production values:

```env
# MongoDB Configuration
MONGO_USERNAME=admin
MONGO_PASSWORD=<generate-strong-password>
MONGO_CONNECTION_STRING=mongodb://admin:<password>@mongodb:27017

# JWT Configuration
JWT_SECRET_KEY=<generate-32-char-secret>

# API Configuration
API_URL=https://api.yourdomain.com

# Or for same server
# API_URL=https://yourdomain.com/api
```

**Generate strong secrets:**
```bash
# On Linux/Mac
openssl rand -base64 32

# On Windows PowerShell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
```

### Step 2: Update Docker Compose for Production

Use the production override:

```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### Step 3: Configure SSL/HTTPS

#### Option A: Using Nginx Reverse Proxy

Create `nginx-proxy.conf`:

```nginx
server {
    listen 80;
    server_name yourdomain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name yourdomain.com;

    ssl_certificate /etc/nginx/certs/fullchain.pem;
    ssl_certificate_key /etc/nginx/certs/privkey.pem;

    # Frontend
    location / {
        proxy_pass http://localhost:3000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }

    # API
    location /api/ {
        proxy_pass http://localhost:5175/api/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # SignalR WebSocket
    location /taskhub/ {
        proxy_pass http://localhost:5175/taskhub/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

#### Option B: Using Let's Encrypt with Certbot

```bash
# Install certbot
sudo apt-get install certbot

# Generate certificate
sudo certbot certonly --standalone -d yourdomain.com -d api.yourdomain.com

# Certificates will be in /etc/letsencrypt/live/yourdomain.com/
```

### Step 4: Deploy

```bash
# Build images
docker-compose build

# Start services
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# Verify all services are running
docker-compose ps

# Check logs
docker-compose logs -f
```

### Step 5: Configure Firewall

```bash
# Allow HTTP and HTTPS
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Allow SSH (if not already allowed)
sudo ufw allow 22/tcp

# Enable firewall
sudo ufw enable
```

---

## Cloud Platform Deployment

### Azure Container Instances

#### 1. Build and Push Images to Azure Container Registry

```bash
# Login to Azure
az login

# Create resource group
az group create --name taskflow-rg --location eastus

# Create Azure Container Registry
az acr create --resource-group taskflow-rg --name taskflowregistry --sku Basic

# Login to ACR
az acr login --name taskflowregistry

# Tag images
docker tag taskflow-api:latest taskflowregistry.azurecr.io/taskflow-api:latest
docker tag taskflow-client:latest taskflowregistry.azurecr.io/taskflow-client:latest

# Push images
docker push taskflowregistry.azurecr.io/taskflow-api:latest
docker push taskflowregistry.azurecr.io/taskflow-client:latest
```

#### 2. Deploy MongoDB (Azure Cosmos DB)

```bash
az cosmosdb create \
  --name taskflow-mongodb \
  --resource-group taskflow-rg \
  --kind MongoDB \
  --server-version 4.0

# Get connection string
az cosmosdb keys list \
  --name taskflow-mongodb \
  --resource-group taskflow-rg \
  --type connection-strings
```

#### 3. Deploy API Container

```bash
az container create \
  --resource-group taskflow-rg \
  --name taskflow-api \
  --image taskflowregistry.azurecr.io/taskflow-api:latest \
  --cpu 1 \
  --memory 1 \
  --registry-login-server taskflowregistry.azurecr.io \
  --registry-username <username> \
  --registry-password <password> \
  --dns-name-label taskflow-api \
  --ports 5175 \
  --environment-variables \
    ASPNETCORE_ENVIRONMENT=Production \
    DatabaseSettings__ConnectionString=<mongodb-connection-string> \
    JwtSettings__SecretKey=<jwt-secret>
```

#### 4. Deploy Frontend Container

```bash
az container create \
  --resource-group taskflow-rg \
  --name taskflow-client \
  --image taskflowregistry.azurecr.io/taskflow-client:latest \
  --cpu 0.5 \
  --memory 0.5 \
  --registry-login-server taskflowregistry.azurecr.io \
  --registry-username <username> \
  --registry-password <password> \
  --dns-name-label taskflow \
  --ports 80
```

### AWS ECS/Fargate

#### 1. Push to Amazon ECR

```bash
# Login to AWS
aws configure

# Create ECR repositories
aws ecr create-repository --repository-name taskflow-api
aws ecr create-repository --repository-name taskflow-client

# Get login credentials
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin <account-id>.dkr.ecr.us-east-1.amazonaws.com

# Tag images
docker tag taskflow-api:latest <account-id>.dkr.ecr.us-east-1.amazonaws.com/taskflow-api:latest
docker tag taskflow-client:latest <account-id>.dkr.ecr.us-east-1.amazonaws.com/taskflow-client:latest

# Push images
docker push <account-id>.dkr.ecr.us-east-1.amazonaws.com/taskflow-api:latest
docker push <account-id>.dkr.ecr.us-east-1.amazonaws.com/taskflow-client:latest
```

#### 2. Create ECS Task Definition

Create `task-definition.json`:

```json
{
  "family": "taskflow",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "512",
  "memory": "1024",
  "containerDefinitions": [
    {
      "name": "taskflow-api",
      "image": "<account-id>.dkr.ecr.us-east-1.amazonaws.com/taskflow-api:latest",
      "portMappings": [
        {
          "containerPort": 5175,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        }
      ]
    }
  ]
}
```

#### 3. Create ECS Service

```bash
aws ecs create-service \
  --cluster taskflow-cluster \
  --service-name taskflow-api \
  --task-definition taskflow:1 \
  --desired-count 2 \
  --launch-type FARGATE
```

### DigitalOcean App Platform

#### 1. Push to Docker Hub

```bash
# Login to Docker Hub
docker login

# Tag images
docker tag taskflow-api:latest <dockerhub-username>/taskflow-api:latest
docker tag taskflow-client:latest <dockerhub-username>/taskflow-client:latest

# Push images
docker push <dockerhub-username>/taskflow-api:latest
docker push <dockerhub-username>/taskflow-client:latest
```

#### 2. Create App via DigitalOcean Console

1. Go to DigitalOcean App Platform
2. Create new app
3. Choose "Docker Hub" as source
4. Add your images
5. Configure environment variables
6. Deploy

Or use `doctl`:

```bash
# Install doctl
# Configure authentication

doctl apps create --spec app-spec.yaml
```

---

## Kubernetes Deployment

### Prerequisites
- Kubernetes cluster (EKS, AKS, GKE, or local minikube)
- kubectl configured
- Images pushed to container registry

### 1. Apply Kubernetes Manifests

```bash
# Navigate to k8s directory
cd k8s/

# Create namespace
kubectl create namespace taskflow

# Create secrets
kubectl create secret generic taskflow-secrets \
  --from-literal=mongodb-password=<password> \
  --from-literal=jwt-secret=<secret> \
  --namespace=taskflow

# Apply all manifests
kubectl apply -f deployment.yml

# Check status
kubectl get pods -n taskflow
kubectl get services -n taskflow
```

### 2. Configure Ingress (Optional)

Install Nginx Ingress Controller:

```bash
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.1/deploy/static/provider/cloud/deploy.yaml
```

Install Cert-Manager for SSL:

```bash
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml
```

### 3. Scale Services

```bash
# Scale API
kubectl scale deployment taskflow-api --replicas=5 -n taskflow

# Scale Client
kubectl scale deployment taskflow-client --replicas=3 -n taskflow

# Check status
kubectl get pods -n taskflow
```

### 4. Monitor Deployment

```bash
# View logs
kubectl logs -f deployment/taskflow-api -n taskflow

# Describe pod
kubectl describe pod <pod-name> -n taskflow

# Get events
kubectl get events -n taskflow --sort-by='.lastTimestamp'
```

---

## Monitoring & Maintenance

### Health Checks

```bash
# Check API health
curl http://localhost:5175/api/test

# Check all container health
docker-compose ps
```

### View Logs

```bash
# Docker Compose
docker-compose logs -f

# Docker
docker logs -f taskflow-api

# Kubernetes
kubectl logs -f deployment/taskflow-api -n taskflow
```

### Backup MongoDB

```bash
# Docker Compose
docker-compose exec mongodb mongodump --out /backup --username admin --password taskflow2025

# Copy backup to host
docker cp taskflow-mongodb:/backup ./mongodb-backup

# Kubernetes
kubectl exec -it mongodb-0 -n taskflow -- mongodump --out /backup
```

### Restore MongoDB

```bash
# Docker Compose
docker-compose exec mongodb mongorestore /backup --username admin --password taskflow2025

# Kubernetes
kubectl exec -it mongodb-0 -n taskflow -- mongorestore /backup
```

### Update Deployment

```bash
# Build new images
docker-compose build

# Push to registry (if using remote registry)
docker push <registry>/taskflow-api:latest
docker push <registry>/taskflow-client:latest

# Restart services
docker-compose up -d

# Or for Kubernetes
kubectl rollout restart deployment/taskflow-api -n taskflow
kubectl rollout restart deployment/taskflow-client -n taskflow
```

### Scaling

```bash
# Docker Compose (manual)
docker-compose up -d --scale api=3

# Kubernetes
kubectl scale deployment taskflow-api --replicas=5 -n taskflow
```

---

## Troubleshooting

### Container Won't Start

```bash
# Check logs
docker-compose logs <service-name>

# Inspect container
docker inspect <container-name>

# Check if port is already in use
netstat -ano | findstr :5175
```

### MongoDB Connection Issues

```bash
# Test MongoDB connection
docker-compose exec mongodb mongosh -u admin -p taskflow2025

# Check if MongoDB is healthy
docker-compose ps mongodb

# Restart MongoDB
docker-compose restart mongodb
```

### API Not Responding

```bash
# Check API logs
docker-compose logs api

# Test API endpoint
curl http://localhost:5175/api/test

# Enter API container
docker-compose exec api bash

# Check environment variables
docker-compose exec api env
```

### Frontend Build Issues

```bash
# Rebuild with no cache
docker-compose build --no-cache client

# Check nginx logs
docker-compose logs client

# Enter client container
docker-compose exec client sh
```

---

## Security Checklist

Before deploying to production:

- [ ] Change all default passwords
- [ ] Use strong JWT secret (32+ characters)
- [ ] Enable HTTPS with valid SSL certificates
- [ ] Configure CORS for your domain only
- [ ] Implement rate limiting
- [ ] Add file upload validation (type, size)
- [ ] Regular security updates
- [ ] Use environment variables for secrets
- [ ] Enable MongoDB authentication
- [ ] Set up monitoring and alerting
- [ ] Configure firewall rules
- [ ] Implement backup strategy
- [ ] Test disaster recovery plan
- [ ] Review application logs regularly
- [ ] Keep Docker images updated

---

## Cost Estimation

### Small Deployment (< 100 users)

**Azure:**
- Container Instances (2 instances): $30/month
- Cosmos DB (Serverless): $25/month
- Storage: $5/month
- **Total: ~$60/month**

**AWS:**
- Fargate (2 tasks): $35/month
- DocumentDB (t3.medium): $60/month
- S3 Storage: $5/month
- **Total: ~$100/month**

**DigitalOcean:**
- App Platform (Basic): $12/month
- Managed MongoDB (1GB): $15/month
- Spaces (Storage): $5/month
- **Total: ~$32/month**

### Medium Deployment (100-1000 users)

**Azure:**
- App Service (P1v2): $78/month
- Cosmos DB (Provisioned): $120/month
- Blob Storage + CDN: $20/month
- **Total: ~$218/month**

### Large Deployment (1000+ users)

- Kubernetes cluster: $150-500/month
- Managed MongoDB: $200-500/month
- Load balancers: $20-50/month
- Storage + CDN: $50-200/month
- **Total: ~$420-1250/month**

---

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [MongoDB Atlas](https://www.mongodb.com/cloud/atlas)
- [Azure Container Instances](https://azure.microsoft.com/services/container-instances/)
- [AWS ECS](https://aws.amazon.com/ecs/)
- [DigitalOcean App Platform](https://www.digitalocean.com/products/app-platform/)

---

## Support

For deployment issues, consult:
- Project README.md
- SPECIFICATIONS.md for technical details
- GitHub Issues
- support@taskflow.com
