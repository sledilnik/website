FROM node:14.14.0-buster-slim

WORKDIR /app

RUN DEBIAN_FRONTEND=noninteractive apt-get update -y \
  && apt-get install -y curl wget apt-transport-https gnupg2 \
  && wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - \
  && sh -c 'echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list' \
  && curl -LO https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb \
  && dpkg -i packages-microsoft-prod.deb && rm packages-microsoft-prod.deb \
  && apt-get update -y \
  && apt-get install -y dotnet-sdk-5.0 google-chrome-unstable libxss1 libx11-6 \
  && apt-get purge -y curl wget apt-transport-https \
  && apt-get clean && rm -rf /var/lib/apt/lists/*