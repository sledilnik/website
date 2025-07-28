########################################
# Build stage - uses native build platform for better performance
########################################
FROM --platform=$BUILDPLATFORM node:20-alpine3.22 AS builder

# Install dependencies needed for .NET SDK on Alpine
RUN apk add --no-cache \
    bash \
    icu-libs \
    krb5-libs \
    libgcc \
    libintl \
    libssl3 \
    libstdc++ \
    zlib \
    wget \
    ca-certificates

# Install .NET 8 SDK manually for multi-arch support
ARG TARGETARCH
RUN case ${TARGETARCH} in \
        "amd64") DOTNET_ARCH=x64 ;; \
        "arm64") DOTNET_ARCH=arm64 ;; \
        *) echo "Unsupported architecture: ${TARGETARCH}" && exit 1 ;; \
    esac \
    && wget -O dotnet.tar.gz "https://download.visualstudio.microsoft.com/download/pr/9c8b7bf6-fbf3-451b-925d-dd5d9473cc84/be4015516d88ad6a38b85f86b7d8db0b/dotnet-sdk-8.0.404-linux-musl-${DOTNET_ARCH}.tar.gz" \
    && mkdir -p /usr/share/dotnet \
    && tar -xzf dotnet.tar.gz -C /usr/share/dotnet \
    && rm dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && apk del wget

ARG BUILD_MODE=production

WORKDIR /app

# Copy package files first for better caching
COPY package.json yarn.lock ./
RUN yarn install --frozen-lockfile

# Copy source code
COPY . .

# Build the application
RUN CADDY_BUILD=1 yarn build --mode ${BUILD_MODE}

########################################
# Final webserver image
########################################
FROM caddy:2.6.4-alpine

WORKDIR /app
COPY --from=builder /app/dist /app
COPY Caddyfile /etc/caddy/Caddyfile