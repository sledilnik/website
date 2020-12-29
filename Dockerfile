########################################
# Intermediate builder image (used only for build, discarded in final stage)
########################################
FROM ghcr.io/sledilnik/website-base:v2 AS builder

ARG BUILD_MODE=production

ADD . /app
RUN yarn
RUN yarn build --mode ${BUILD_MODE}

########################################
# Actual webserver image
########################################
FROM caddy:2.2.1-alpine

WORKDIR /app
COPY /dist /app
COPY Caddyfile /etc/caddy/Caddyfile