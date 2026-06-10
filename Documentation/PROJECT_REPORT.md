# Travel Agency Technical Report

## 1. Executive Summary

Travel Agency is a React frontend and ASP.NET Core Web API backend project for managing destinations, hotels, travel packages, bookings, user accounts, and contact messages. The system exposes versioned REST endpoints under `/api/v1`, uses JWT authentication, role-based authorization, Swagger/OpenAPI documentation, SQL Server persistence, and Docker-based execution.

The project also includes advanced requirement coverage: MFA for protected roles with queue/email delivery, Redis-ready caching, RabbitMQ asynchronous processing, Prometheus metrics, audit logging, NGINX gateway/load balancing, ELK log shipping, Alertmanager/Prometheus/Grafana infrastructure, Kubernetes manifests, automated backup jobs, MongoDB analytics, S3-compatible storage, and Playwright E2E tests.

## 2. Project Goal and Objectives

The goal is to build a functional travel-agency portal where users browse destinations and packages, make bookings, manage profiles, and contact the agency, while administrators manage operational data through protected endpoints and frontend tools.

### Objectives

- Build a real REST API using ASP.NET Core Web API.
- Build a React frontend that consumes the API.
- Use JWT and RBAC for stateless authentication and authorization.
- Add MFA for sensitive roles such as admin and auditor.
- Use SQL Server as the primary relational database.
- Document the API through OpenAPI/Swagger UI.
- Demonstrate microservices through a notification service.
- Demonstrate queue-based async processing through RabbitMQ.
- Support local, Docker Compose, and Kubernetes deployment paths.
- Add automated tests, linting, CI, and browser E2E coverage.

## 3. Functional and Non-Functional Requirements

### Functional Requirements

- User registration and login
- MFA verification for configured roles
- Forgot-password and reset-password flow
- User profile update and password change
- Public destination and package browsing
- Booking creation, history, details, and cancellation
- Admin management for places, hotels, packages, bookings, users, and contact messages
- Contact form submission
- Infrastructure endpoints for audit logs, metrics snapshots, analytics events, and storage demo
- Notification microservice for queued MFA delivery

### Non-Functional Requirements

- JWT authentication and role-based authorization
- Parameterized SQL and input validation
- Swagger/OpenAPI documentation with examples
- Redis-ready distributed caching
- RabbitMQ-based asynchronous processing
- Prometheus metrics and Alertmanager alerting
- ELK-compatible centralized log shipping
- Docker Compose enterprise stack
- Kubernetes manifests with API replicas, HPA, ingress, and backup CronJob
- Automated backend, frontend, and E2E tests

## 4. System Design

The system now uses a small microservice split:

- Main API: `Travel Agency Portal/Travel Agency Portal`
- Notification microservice: `Microservices/NotificationService`
- Frontend: `UI/travel-agency`
- Database: SQL Server
- Queue: RabbitMQ
- Cache: Redis
- Observability: Prometheus, Grafana, Alertmanager, ELK

The main API owns the travel-agency business workflows. The notification service consumes RabbitMQ messages for MFA delivery and sends email through SMTP when configured. This gives the project a concrete microservice and queue-based async-processing example while keeping the rest of the coursework app understandable.

## 5. Implementation Description

The backend includes JWT authentication, RBAC, MFA challenge/verification, Swagger examples, health checks, `/metrics`, caching, rate limiting, request logging, exception handling, audit logging, EF Core support tables, hosted workers, RabbitMQ publishing, Logstash log shipping, email/webhook alerting, MongoDB analytics, and S3-compatible storage.

The frontend includes public pages, package/destination pages, login/register, forgot password, MFA login step, dashboards, profile, bookings, admin management, and contact form.

The enterprise stack includes two API instances behind NGINX, Redis, RabbitMQ, notification service, SQL Server, MongoDB, MinIO, ELK, Prometheus, Grafana, Alertmanager, and a SQL Server backup helper.

## 6. Security and Technical Choices

REST was chosen because the application is resource-oriented and serves a JavaScript frontend. JSON is lightweight and easy to test through Swagger.

JWT supports stateless authentication through the `Authorization: Bearer <token>` header. RBAC separates normal user permissions, admin management operations, and auditor read-only review access.

Security measures include password hashing, parameterized SQL, input validation, rate limiting, audit logging, MFA, HTTPS redirection, and production-style configuration for secrets and alerts.

The MFA code can be exposed in local demo mode for classroom testing. In production-style configuration, `Security__ExposeMfaCodeInResponse=false` and the code is delivered through RabbitMQ/email.

## 7. API Documentation and Infrastructure

Swagger UI includes bearer authentication, request examples, response examples, and common error response descriptions. The API is versioned under `/api/v1`.

Infrastructure support includes:

- `/health`
- `/metrics`
- NGINX gateway
- Redis caching
- RabbitMQ queue
- Notification microservice
- ELK log shipping
- Prometheus/Grafana/Alertmanager
- Kubernetes manifests
- SQL Server backup automation
- MongoDB analytics endpoint
- S3-compatible storage endpoint

## 8. Testing and Results

Backend tests cover health checks, authentication protection, password hashing, and infrastructure behavior. Frontend checks cover Jest tests, linting, and production build. Playwright E2E tests cover browser-level public navigation and authentication-form visibility.

Manual API testing is documented in `Documentation/API_TESTS.md`.

## 9. DevOps and Delivery

The repository includes:

- `Dockerfile`
- `docker-compose.yml`
- `docker-compose.enterprise.yml`
- `Microservices/NotificationService`
- `Infrastructure/kubernetes`
- `Infrastructure/backups/sqlserver-backup.sh`
- backend CI workflow
- frontend CI workflow
- E2E CI workflow
- ESLint, Prettier, and `.editorconfig`
- report and requirement documentation

## 10. Conclusions and Recommendations

The project now demonstrates all major coursework requirements through working code, configuration, tests, or runnable infrastructure scaffolding. It is still not a fully operated production system because real deployment would require private secrets, hosted domains, TLS certificates, real SMTP/webhook credentials, cloud accounts, and production monitoring dashboards.

Recommended future improvements are to expand Playwright tests for authenticated booking/admin flows, add cloud-provider-specific deployment scripts, configure real secret management, and perform restore drills using real backup files.

## 11. References

- Microsoft ASP.NET Core Documentation
- Microsoft JWT Bearer Authentication Documentation
- OpenAPI Specification / Swagger Documentation
- React Documentation
- Docker Documentation
- Kubernetes Documentation
- SQL Server Documentation
- RabbitMQ Documentation
- Prometheus and Grafana Documentation

## 12. Appendices

### Appendix A: Main Project Paths

- Backend: `Travel Agency Portal/Travel Agency Portal`
- Notification microservice: `Microservices/NotificationService`
- Backend tests: `Travel Agency Portal/Travel Agency Portal.Tests`
- Frontend: `UI/travel-agency`
- E2E tests: `UI/travel-agency/tests/e2e`
- Database scripts: `Travel_AgencyDB.sql`, `Travel_Agency_Features_Update.sql`
- Advanced infrastructure: `docker-compose.enterprise.yml`, `Infrastructure/`

### Appendix B: Originality Declaration Template

I declare that this project is my own work (or the work of my group), prepared for academic purposes for the course on Web Services and Web API. Any external materials or references used are properly acknowledged in the report and supporting documentation.
