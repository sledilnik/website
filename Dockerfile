########################################
# Intermediate builder image (used only for build, discarded in final stage)
########################################
FROM ghcr.io/sledilnik/website-base AS builder

ARG BUILD_MODE=production

ADD . /app
RUN yarn
RUN CADDY_BUILD=1 yarn build --mode ${BUILD_MODE}

########################################
# Actual webserver image
########################################
FROM caddy:2.8.4-alpine

WORKDIR /app
COPY --from=builder /app/dist /app
COPY Caddyfile /etc/caddy/Caddyfile