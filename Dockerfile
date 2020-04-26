FROM sledilnik/web-builder:latest AS builder

FROM caddy:2.0.0-rc.3-alpine

COPY --from=builder /app/dist /app

COPY Caddyfile /etc/caddy/Caddyfile