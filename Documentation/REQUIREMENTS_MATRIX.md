# Travel Agency Requirements Matrix

This document maps the project against the PDF requirements in `Kerkesat teknike per implementimin e projekteve ne Ueb Sherbime dhe Ueb API.pdf`.

## Summary

The project now covers the core Web API coursework requirements and the advanced infrastructure requirements through code, configuration, tests, Docker Compose, Kubernetes manifests, and documentation.

## Requirement Status

| Requirement | Status | Current Evidence | Notes |
| --- | --- | --- | --- |
| Modular architecture | Yes | `Controllers`, `Services`, `Middleware`, `Filters`, `Data` | Clear backend separation of concerns |
| Microservices architecture | Yes | `Microservices/NotificationService` | MFA delivery is separated into an independent notification microservice |
| REST API with JSON | Yes | `Controllers/*.cs` | Uses standard HTTP methods and JSON responses |
| Stateless communication | Yes | JWT bearer auth in [`Program.cs`](C:/Users/Lenovo/OneDrive/Desktop/Travel%20Agency/Travel%20Agency%20Portal/Travel%20Agency%20Portal/Program.cs) | No server-side session dependency |
| JWT authentication | Yes | `AuthController`, `JwtTokenService` | Login/register return JWT |
| RBAC | Yes | `[Authorize(Roles = "...")]` | Admin and user roles are enforced |
| MFA | Yes | `MfaService`, `MfaDeliveryService`, `NotificationService`, `User_Mfa_Codes` | Local demo can expose the code; production config delivers by queue/email |
| HTTPS/TLS | Partial | `app.UseHttpsRedirection()` | App-level HTTPS redirection exists; production certificates are environment-specific |
| SQL injection protection | Yes | Parameterized queries in controllers/services | ADO.NET parameters are used |
| XSS / CSRF / WAF | Partial | React escaping, validation, bearer-token auth, NGINX gateway | No full WAF appliance, but gateway scaffolding exists |
| Auditing / secure logging | Yes | `AuditLogService`, `Audit_Logs`, request logging middleware | Audit data is persisted |
| Advanced caching | Yes | Redis configuration + distributed cache | Places/packages support distributed cache with memory fallback |
| Load balancing | Yes | NGINX gateway, `api1`, `api2`, Kubernetes Service/HPA | Docker and Kubernetes examples included |
| Async processing / queues | Yes | RabbitMQ, `MfaDeliveryService`, `NotificationService` | MFA delivery is published to a queue and processed out of band |
| Monitoring resources | Yes | `/metrics`, Prometheus config, metrics snapshots | Prometheus-compatible metrics included |
| OpenAPI / Swagger UI | Yes | Swagger config + `SwaggerExamplesOperationFilter` | Interactive docs include examples and error responses |
| API versioning | Yes | `/api/v1/...` routes | URL versioning applied |
| Changelog / communication of changes | Yes | [`CHANGELOG.md`](C:/Users/Lenovo/OneDrive/Desktop/Travel%20Agency/CHANGELOG.md) | Change history included |
| Centralized logging (ELK / Graylog) | Yes | `LogstashLoggerProvider`, Logstash/Elasticsearch/Kibana config | App logs can be shipped to Logstash over TCP as JSON |
| Alerting | Yes | `AlertingService`, Prometheus rules, Alertmanager config | Slack/webhook and email alert channels are configurable |
| API Gateway | Yes | `Infrastructure/nginx/nginx.conf` | Gateway handles proxying, rate limiting, and load balancing |
| Relational database integration | Yes | `Travel_AgencyDB.sql` | SQL Server relational schema |
| ORM | Yes | `TravelAgencyDbContext` | EF Core is used for support/audit tables |
| NoSQL integration | Yes | `MongoAnalyticsService` | Optional MongoDB analytics storage |
| Cloud storage | Yes | `S3StorageService`, MinIO setup | S3-compatible storage demo included |
| Clean code / structure | Partial | Separated controllers/services/middleware/tests/docs | Strong coursework structure with room for more refactoring |
| Linting / formatting tools | Yes | ESLint, Prettier, `.editorconfig` | Frontend lint/format and repo formatting included |
| Unit tests | Yes | `Travel Agency Portal.Tests` | Backend tests exist |
| Integration tests | Partial | Infrastructure/auth checks + manual API tests | Some integration coverage exists |
| End-to-end tests | Yes | Playwright config and `UI/travel-agency/tests/e2e` | Browser E2E suite and workflow added |
| Continuous testing in CI/CD | Yes | Backend, frontend, and E2E workflows | Build/test automation exists |
| React frontend | Yes | `UI/travel-agency` | Implemented |
| Docker / Docker Compose | Yes | `docker-compose.yml`, `docker-compose.enterprise.yml` | Basic and advanced stacks included |
| Kubernetes | Yes | `Infrastructure/kubernetes/*.yaml` | Deployments, services, ingress, HPA, secrets/config, and backup CronJob included |
| Backup and recovery policies | Yes | `BACKUP_RECOVERY.md`, `Infrastructure/backups/sqlserver-backup.sh`, Kubernetes CronJob | Documented and automated examples included |
| Academic technical report | Yes | `PROJECT_REPORT.md` + generated `.docx` | Structured for coursework submission |

## Honest Final Position

The project now has implementation evidence for every major requirement. Some items are still demo or environment-ready rather than fully production-operated, because real production deployment would require private secrets, real domains, TLS certificates, SMTP credentials, Slack/email webhooks, cloud accounts, and hosted infrastructure.
