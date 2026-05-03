FROM node:lts-alpine AS builder
WORKDIR /app
COPY package.json ./
COPY package-lock.json ./

RUN npm install
COPY . .
RUN cp src/env.ts.template src/env.ts
RUN npm run build

FROM nginx:stable-alpine-slim AS runtime
RUN apk add --no-cache gettext
COPY --from=builder /app/.nginx/nginx.conf.template /etc/nginx/conf.d/default.conf.template
COPY --from=builder /app/.nginx/docker-entrypoint.sh /docker-entrypoint.sh
COPY --from=builder /app/scripts/generate-env.sh /scripts/generate-env.sh
RUN chmod +x /docker-entrypoint.sh /scripts/generate-env.sh
WORKDIR /usr/share/nginx/html
RUN rm -rf ./*
COPY --from=builder /app/dist .

ENTRYPOINT ["/docker-entrypoint.sh"]
