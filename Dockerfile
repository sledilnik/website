FROM caddy:2.2.1-alpine

WORKDIR /app
COPY /dist /app
COPY Caddyfile /etc/caddy/Caddyfile