########################################
# builder image
########################################
FROM docker.pkg.github.com/sledilnik/docker-base/web-base:latest AS builder
ADD . /app
RUN yarn
RUN NODE_ENV=production yarn build

########################################
# webserver image
########################################
FROM caddy:2.0.0-rc.3-alpine

WORKDIR /app
COPY --from=builder /app/dist /app
COPY Caddyfile /etc/caddy/Caddyfile