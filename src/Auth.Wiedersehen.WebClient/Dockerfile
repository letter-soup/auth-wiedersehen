FROM node:lts-alpine AS builder
WORKDIR /app
COPY package.json ./
COPY package-lock.json ./

RUN npm install
COPY . .
RUN npm run build

FROM nginx:stable-alpine-slim AS runtime
COPY --from=builder /app/.nginx/nginx.conf /etc/nginx/conf.d/default.conf
WORKDIR /usr/share/nginx/html
RUN rm -rf ./*
COPY --from=builder /app/dist .

ENTRYPOINT ["nginx", "-g", "daemon off;"]
