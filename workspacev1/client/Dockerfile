# Build stage
FROM node:18-alpine AS build
WORKDIR /app

# 1) Accept build arg for CRA env
ARG REACT_APP_API_URL
ENV REACT_APP_API_URL=${REACT_APP_API_URL}

COPY package*.json ./

# Install dependencies
RUN npm ci --production=false

# Copy source code
COPY . .

# 2) CRA will read REACT_APP_* here
RUN npm run build

# Production stage with nginx
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Remove default nginx static content
RUN rm -rf ./*

# Copy built app from build stage
COPY --from=build /app/build .

# Copy nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
