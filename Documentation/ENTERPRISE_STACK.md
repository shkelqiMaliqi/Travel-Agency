# Enterprise Feature Demo Stack

This project now includes a **demo-level enterprise stack** for coursework purposes in:

- [`docker-compose.enterprise.yml`](C:/Users/Lenovo/OneDrive/Desktop/Travel%20Agency/docker-compose.enterprise.yml)

## Included Services

- `api1`, `api2`
  Two ASP.NET Core API instances for load-balancing demonstrations.
- `notification-service`
  Separate ASP.NET Core microservice that consumes RabbitMQ MFA delivery messages and sends email.
- `gateway`
  NGINX reverse proxy acting as API Gateway and load balancer.
- `redis`
  Advanced distributed caching backend.
- `rabbitmq`
  Message broker for asynchronous MFA delivery.
- `mongo`
  NoSQL service for analytics events.
- `minio`
  S3-compatible object storage used as a cloud-storage demo.
- `elasticsearch`, `logstash`, `kibana`
  ELK stack for centralized logging demonstrations.
- `prometheus`
  Metrics scraping for `/metrics`.
- `grafana`
  Dashboard visualization layer.
- `alertmanager`
  Alert-routing component.
- `sqlserver`
  Primary relational database.
- `sqlserver-backup`
  SQL Server backup helper for automated backup demonstrations.

## What This Proves

This stack gives the repository concrete implementation evidence for:

- load balancing
- API gateway
- Redis caching
- microservice separation
- RabbitMQ async processing
- monitoring stack
- centralized logging stack
- alerting infrastructure
- automated SQL Server backups
- NoSQL integration
- S3-compatible cloud storage integration

## Kubernetes

Kubernetes manifests are available in:

- [`Infrastructure/kubernetes`](C:/Users/Lenovo/OneDrive/Desktop/Travel%20Agency/Infrastructure/kubernetes)

They include API replicas, notification-service deployment, Redis, RabbitMQ, SQL Server, ingress, HPA, configuration/secret examples, and a backup CronJob.

## How to Run

```powershell
docker compose -f docker-compose.enterprise.yml up --build
```

## Important Note

This is a **coursework/demo infrastructure layer**, not a fully hardened production platform. It is designed to satisfy technical requirement coverage and demonstrate architecture concepts in a practical way.
