FROM node:20-alpine AS deps
WORKDIR /app
COPY bookquotes-ui/package*.json ./
RUN npm ci --legacy-peer-deps

FROM node:20-alpine AS build
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY bookquotes-ui/ .
RUN npm run build -- --output-path=./dist --configuration=production

# Serve with a lightweight static server
FROM nginx:alpine
COPY --from=build /app/dist/browser/ /usr/share/nginx/html
EXPOSE 80
CMD ["/bin/sh", "-c", "nginx -g 'daemon off;'"]
