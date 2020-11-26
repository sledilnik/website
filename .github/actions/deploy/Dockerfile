FROM alpine/helm:3.4.1

RUN apk add --no-update curl

RUN helm repo add sledilnik https://helm-repo.sledilnik.org && helm repo update

COPY entrypoint.sh /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]