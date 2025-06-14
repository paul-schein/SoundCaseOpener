FROM node:23-alpine3.21 AS builder

WORKDIR /app

COPY package*.json ./

RUN npm install

COPY . .

RUN npm run build --configuration sound-case-opener

FROM caddy:latest AS production

COPY --from=builder /app/dist/sound-case-opener /var/www/html

RUN apk update && \
    rm -rf /var/cache/apk/*

RUN rm /etc/caddy/Caddyfile
COPY Caddyfile /etc/caddy/Caddyfile

EXPOSE 80

ENTRYPOINT ["caddy", "run", "--config", "/etc/caddy/Caddyfile"]
