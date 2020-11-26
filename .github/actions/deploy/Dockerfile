FROM alpine/helm:3.4.1

RUN apk add --no-cache nodejs

RUN helm repo add sledilnik https://helm-repo.sledilnik.org && helm repo update

COPY dist/index.js index.js
COPY entrypoint.sh /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]