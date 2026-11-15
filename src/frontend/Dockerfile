FROM node:22-alpine AS build
RUN corepack enable
WORKDIR /app

COPY src/frontend/package.json src/frontend/pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

COPY src/frontend/ ./
RUN pnpm run build

FROM nginx:alpine AS runtime
COPY src/frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
