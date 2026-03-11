# Build stage
FROM node:20 AS build
WORKDIR /app

# Copy package files and install dependencies
COPY package*.json ./
RUN npm install

# Copy source code and build the app
COPY . .
RUN npm run build

# Production stage - using Nginx to serve the static files
FROM nginx:alpine
COPY --from=build /app/dist/client/browser /usr/share/nginx/html

# Copy custom nginx config if you have one, or use a basic one to handle routing
RUN echo 'server { \
    listen 4200; \
    location / { \
    root /usr/share/nginx/html; \
    index index.html index.htm; \
    try_files $uri $uri/ /index.html; \
    } \
    }' > /etc/nginx/conf.d/default.conf

EXPOSE 4200
CMD ["nginx", "-g", "daemon off;"]
