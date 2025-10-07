########################################
# Intermediate builder image (used only for build, discarded in final stage)
########################################
FROM bitnami/dotnet-sdk:8-debian-11 as builder
ARG NODE_MAJOR=18

WORKDIR /app

ARG BUILD_MODE=production

RUN apt-get update && apt-get install -y ca-certificates curl gnupg && mkdir -p /etc/apt/keyrings && \
    curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg;

RUN echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_$NODE_MAJOR.x nodistro main" > /etc/apt/sources.list.d/nodesource.list;

RUN apt-get update && apt-get install nodejs -y && npm install --global yarn

ADD . /app
RUN yarn
RUN CADDY_BUILD=1 yarn build --mode ${BUILD_MODE}

########################################
# Actual webserver image
########################################
FROM caddy:2.6.4-alpine

WORKDIR /app
COPY --from=builder /app/dist /app
COPY Caddyfile /etc/caddy/Caddyfile