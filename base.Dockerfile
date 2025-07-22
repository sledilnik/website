FROM node:20.1.0-buster-slim

WORKDIR /app

RUN DEBIAN_FRONTEND=noninteractive apt-get update -y && apt-get install -y curl wget apt-transport-https

RUN curl -LO https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb && \
  dpkg -i packages-microsoft-prod.deb && rm packages-microsoft-prod.deb

RUN DEBIAN_FRONTEND=noninteractive apt-get update -y && apt-get install -y dotnet-sdk-7.0

RUN apt-get purge -y curl wget apt-transport-https && apt-get clean && rm -rf /var/lib/apt/lists/*