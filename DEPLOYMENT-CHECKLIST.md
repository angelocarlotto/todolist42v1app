# 🚀 TaskFlow - Deployment Checklist

Use this checklist to ensure a smooth deployment to production.

---

## 📋 Pre-Deployment Checklist

### 🔧 Environment Setup

- [ ] Docker and Docker Compose installed on deployment server
- [ ] Domain name purchased and DNS configured
- [ ] SSL certificates obtained (Let's Encrypt or purchased)
- [ ] Server firewall configured (ports 80, 443, 22)
- [ ] Backup storage configured (for MongoDB backups)

### 🔐 Security Configuration

- [ ] **Changed default MongoDB password**
  ```bash
  # Generate strong password
  openssl rand -base64 32
  ```
- [ ] **Generated strong JWT secret (32+ characters)**
  ```bash
  openssl rand -base64 32
  ```
- [ ] **Created `.env` file with production values**
  ```bash
  cp .env.example .env
  # Edit .env with secure values
  ```
- [ ] **Removed/disabled React StrictMode debug logs** (in Navbar.js)
- [ ] **Configured CORS for production domain**
  ```csharp
  // In Program.cs
  builder.Services.AddCors(options => {
      options.AddPolicy("Production", policy => {
          policy.WithOrigins("https://yourdomain.com")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
      });
  });
  ```
- [ ] **File upload validation implemented**
  - [ ] File size limit (10MB recommended)
  - [ ] File type whitelist
  - [ ] Virus scanning (optional but recommended)

### 🧪 Testing

- [ ] **All features tested in Docker containers**
  ```bash
  docker-compose up -d
  # Test each feature
  ```
- [ ] **SignalR real-time updates working**
- [ ] **File upload/download/delete working**
- [ ] **Comments and activity log working**
- [ ] **Public share links working**
- [ ] **User authentication working**
- [ ] **MongoDB connection working**

### 📝 Documentation

- [ ] **API endpoints documented** (consider adding Swagger)
- [ ] **User guide created**
- [ ] **Admin documentation created**
- [ ] **Troubleshooting guide created**
- [ ] **README.md updated with production URLs**

### 🛠️ Infrastructure

- [ ] **MongoDB backup strategy defined**
  - [ ] Automated daily backups
  - [ ] Backup retention policy (30 days recommended)
  - [ ] Backup restoration tested
- [ ] **Monitoring tools configured**
  - [ ] Application Insights / New Relic / Datadog
  - [ ] Uptime monitoring (Pingdom, Better Uptime)
  - [ ] Error tracking (Sentry)
- [ ] **Logging configured**
  - [ ] Centralized logging (if using multiple servers)
  - [ ] Log retention policy
  - [ ] Log analysis tools

---

## 🚀 Deployment Steps

### Step 1: Prepare Server

- [ ] SSH into deployment server
- [ ] Update system packages
  ```bash
  sudo apt update && sudo apt upgrade -y
  ```
- [ ] Install Docker
  ```bash
  curl -fsSL https://get.docker.com -o get-docker.sh
  sudo sh get-docker.sh
  ```
- [ ] Install Docker Compose
  ```bash
  sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
  sudo chmod +x /usr/local/bin/docker-compose
  ```

### Step 2: Deploy Application

- [ ] Clone repository to server
  ```bash
  git clone <repository-url>
  cd todolist42v1app
  ```
- [ ] Create `.env` file with production values
- [ ] Build Docker images
  ```bash
  docker-compose build
  ```
- [ ] Start services
  ```bash
  docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
  ```
- [ ] Verify all containers running
  ```bash
  docker-compose ps
  ```
- [ ] Check logs for errors
  ```bash
  docker-compose logs -f
  ```

### Step 3: Configure Reverse Proxy (Nginx)

- [ ] Install Nginx
  ```bash
  sudo apt install nginx -y
  ```
- [ ] Create Nginx configuration
  ```bash
  sudo nano /etc/nginx/sites-available/taskflow
  ```
- [ ] Enable site
  ```bash
  sudo ln -s /etc/nginx/sites-available/taskflow /etc/nginx/sites-enabled/
  sudo nginx -t
  sudo systemctl reload nginx
  ```
- [ ] Test HTTP access

### Step 4: Configure SSL/HTTPS

- [ ] Install Certbot
  ```bash
  sudo apt install certbot python3-certbot-nginx -y
  ```
- [ ] Generate SSL certificates
  ```bash
  sudo certbot --nginx -d yourdomain.com -d api.yourdomain.com
  ```
- [ ] Test HTTPS access
- [ ] Verify auto-renewal
  ```bash
  sudo certbot renew --dry-run
  ```

### Step 5: Database Initialization

- [ ] Connect to MongoDB
  ```bash
  docker-compose exec mongodb mongosh -u admin -p <password>
  ```
- [ ] Create initial admin user (if needed)
- [ ] Create test tenant (if needed)
- [ ] Verify database indexes created
  ```javascript
  use TaskManagement
  db.tasks.getIndexes()
  ```

### Step 6: Smoke Tests

- [ ] **Frontend accessible at https://yourdomain.com**
- [ ] **API accessible at https://api.yourdomain.com or https://yourdomain.com/api**
- [ ] **User can register/login**
- [ ] **User can create task**
- [ ] **User can edit task**
- [ ] **User can delete task**
- [ ] **Real-time updates working (open in two browsers)**
- [ ] **File upload working**
- [ ] **File download working**
- [ ] **File delete working**
- [ ] **Comments working**
- [ ] **Public share links working**
- [ ] **SignalR WebSocket connection successful** (check browser console)

---

## 📊 Post-Deployment Checklist

### Monitoring & Alerting

- [ ] **Application monitoring configured**
  - [ ] API response time tracking
  - [ ] Error rate monitoring
  - [ ] Memory and CPU usage
- [ ] **Alerts configured**
  - [ ] API error rate > 5%
  - [ ] API response time > 500ms
  - [ ] Server CPU > 80%
  - [ ] Server memory > 85%
  - [ ] Disk space < 20%
  - [ ] MongoDB connection failures
- [ ] **Uptime monitoring configured**
  - [ ] Ping every 5 minutes
  - [ ] Alert on 3 consecutive failures
- [ ] **Log monitoring configured**
  - [ ] Error logs reviewed daily
  - [ ] Access logs analyzed weekly

### Backups

- [ ] **Automated backup configured**
  ```bash
  # Create backup script
  cat > /home/backups/mongodb-backup.sh << 'EOF'
  #!/bin/bash
  DATE=$(date +%Y%m%d_%H%M%S)
  docker-compose exec -T mongodb mongodump \
    --username admin \
    --password <password> \
    --out /backup/$DATE
  docker cp taskflow-mongodb:/backup/$DATE /home/backups/mongodb/
  # Keep only last 30 days
  find /home/backups/mongodb -mtime +30 -exec rm -rf {} \;
  EOF
  
  chmod +x /home/backups/mongodb-backup.sh
  ```
- [ ] **Backup cron job configured**
  ```bash
  # Run daily at 2 AM
  crontab -e
  0 2 * * * /home/backups/mongodb-backup.sh
  ```
- [ ] **Backup restoration tested**
  ```bash
  docker-compose exec mongodb mongorestore \
    /backup/<backup-date> \
    --username admin \
    --password <password>
  ```
- [ ] **Off-site backup configured** (AWS S3, Azure Blob, etc.)

### Security Hardening

- [ ] **Firewall configured**
  ```bash
  sudo ufw allow 22/tcp   # SSH
  sudo ufw allow 80/tcp   # HTTP
  sudo ufw allow 443/tcp  # HTTPS
  sudo ufw enable
  ```
- [ ] **SSH hardening**
  - [ ] Disable root login
  - [ ] Use SSH keys (disable password auth)
  - [ ] Change default SSH port (optional)
- [ ] **Rate limiting implemented**
  - [ ] API rate limiting (in code or reverse proxy)
  - [ ] Login attempt rate limiting
- [ ] **Security headers configured** (already in Nginx config)
- [ ] **Database accessible only from API container**
- [ ] **Secrets not in source control** (`.env` in `.gitignore`)
- [ ] **Regular security updates scheduled**
  ```bash
  # Create auto-update script
  sudo apt install unattended-upgrades -y
  ```

### Performance Optimization

- [ ] **CDN configured for static assets** (Cloudflare, Azure CDN, AWS CloudFront)
- [ ] **Database indexes verified**
  ```javascript
  db.tasks.createIndex({ tenantId: 1, status: 1, dueDate: 1 })
  db.tasks.createIndex({ tenantId: 1, assignedUsers: 1 })
  db.users.createIndex({ username: 1 }, { unique: true })
  ```
- [ ] **Caching implemented** (Redis for API responses - future enhancement)
- [ ] **Image optimization** (for uploaded files - future enhancement)
- [ ] **Gzip compression enabled** (already in Nginx)

### Documentation Updates

- [ ] **Production URLs updated in README**
- [ ] **Deployment date documented**
- [ ] **Server access credentials securely stored** (password manager)
- [ ] **Runbook created** (step-by-step operations guide)
- [ ] **Incident response plan created**
- [ ] **Contact information for team updated**

---

## 🎯 Week 1 Post-Launch

- [ ] **Day 1:** Monitor logs hourly
- [ ] **Day 2:** Check all alerts and monitoring
- [ ] **Day 3:** Review error logs, fix critical issues
- [ ] **Day 4:** Analyze performance metrics
- [ ] **Day 5:** Test backup restoration
- [ ] **Day 7:** Review user feedback, plan improvements

---

## 📈 Ongoing Maintenance (Monthly)

- [ ] **Review and rotate logs**
- [ ] **Update Docker images** (security patches)
  ```bash
  docker-compose pull
  docker-compose up -d
  ```
- [ ] **Review and delete old backups** (keep 30 days)
- [ ] **Security audit** (check for vulnerabilities)
- [ ] **Performance review** (API response times, error rates)
- [ ] **Capacity planning** (disk space, memory, CPU)
- [ ] **Review monitoring alerts** (false positives?)
- [ ] **Update documentation** (new features, changes)
- [ ] **Team training** (new features, tools)

---

## 🆘 Emergency Contacts

| Role | Name | Contact | Availability |
|------|------|---------|--------------|
| DevOps Lead | [Name] | [Email/Phone] | 24/7 |
| Backend Developer | [Name] | [Email/Phone] | Business hours |
| Frontend Developer | [Name] | [Email/Phone] | Business hours |
| DBA | [Name] | [Email/Phone] | On-call |
| Security Lead | [Name] | [Email/Phone] | On-call |

---

## 📞 Escalation Path

1. **Level 1:** Check logs, restart services
2. **Level 2:** Contact DevOps Lead
3. **Level 3:** Contact Backend/Frontend Developer
4. **Level 4:** Contact Management

---

## ✅ Sign-Off

- [ ] **Technical Lead:** Approved deployment
- [ ] **Security Team:** Security review completed
- [ ] **QA Team:** Testing completed
- [ ] **Product Owner:** Feature verification completed
- [ ] **Operations Team:** Monitoring configured

**Deployment Date:** _______________

**Deployed By:** _______________

**Approved By:** _______________

---

## 🎉 You're Ready!

Once all items are checked, your TaskFlow application is production-ready!

**Remember:**
- Monitor closely in the first week
- Keep backups
- Update regularly
- Document everything
- Communicate with your team

**Good luck! 🚀**
