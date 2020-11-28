FROM alpine/helm:3.4.1

RUN apk add --no-cache nodejs

COPY dist/index.js /index.js
COPY entrypoint.sh /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]