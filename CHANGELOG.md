# Changelog

All notable changes to this coursework project should be documented in this file.

## 2026-06-09

### Added

- Requirements traceability document in `Documentation/REQUIREMENTS_MATRIX.md`
- Report-ready written project documentation in `Documentation/PROJECT_REPORT.md`
- Architecture notes in `Documentation/ARCHITECTURE.md`
- Backup and recovery notes in `Documentation/BACKUP_RECOVERY.md`
- Submission checklist in `Documentation/SUBMISSION_CHECKLIST.md`
- Frontend GitHub Actions workflow for build and test validation
- Enterprise demo stack in `docker-compose.enterprise.yml`
- NGINX gateway/load-balancer configuration
- Redis-ready distributed cache support
- Admin MFA verification flow
- Background cleanup and metrics workers
- Prometheus `/metrics` endpoint support
- Audit log persistence using Entity Framework Core
- MongoDB demo analytics integration
- S3-compatible storage demo integration
- Prettier config, lint script, and `.editorconfig`
- Notification microservice for queued MFA email delivery
- RabbitMQ asynchronous processing support
- Logstash TCP application log shipping
- Production-style email/webhook alerting configuration
- Kubernetes manifests with API replicas, HPA, ingress, RabbitMQ, Redis, SQL Server, and backup CronJob
- Automated SQL Server backup helper script
- Playwright E2E test suite and E2E CI workflow

### Verified

- Backend test suite passes
- Frontend test suite passes
- Frontend production build passes
- Frontend lint passes

## 2026-05-08

### Added

- Expanded README with setup, routes, API endpoints, Docker, tests, and security notes
- Backend CI workflow

## 2026-05-04

### Added

- Dockerfile and docker-compose support
- Expanded SQL schema and feature update script
