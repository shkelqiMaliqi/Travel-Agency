FROM node:22-alpine AS build
WORKDIR /app
COPY UI/travel-agency/package*.json ./
RUN npm ci --no-audit --no-fund
COPY UI/travel-agency/ ./
ARG REACT_APP_API_URL=/api/v1
ENV REACT_APP_API_URL=$REACT_APP_API_URL
RUN npm run build

FROM nginx:1.27-alpine
ENV API_UPSTREAM=gateway
COPY Infrastructure/nginx/frontend.conf /etc/nginx/templates/default.conf.template
COPY --from=build /app/build /usr/share/nginx/html
EXPOSE 80
