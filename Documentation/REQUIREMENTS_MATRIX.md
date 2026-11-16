# Requirements matrix for the supplied 2026 PDF

This matrix uses the six-page `Kërkesat teknike_Ueb Shërbime Ueb APIs 2026.pdf` supplied on 16 November 2026. The older extracted requirements file and prior matrix describe a different document. See `VERIFICATION_2026-11-16.md` and `Evidence/verification-2026-11-16.json` for actual local results.

| Section | Implementation or evidence | Current status |
| --- | --- | --- |
| 1 Purpose and objectives | PROJECT_REPORT.md; functional website/API | Verified core |
| 2 Requirements analysis | Problem, actors, more than five functional requirements, measurable nonfunctional acceptance, business/data flows in report | Documented |
| 3 Architecture and API design | Controllers/services/repositories; 42 REST operations; JSON/OpenAPI; NotificationService boundary | Core verified; queue communication awaits container run |
| 4 Business rules | Validation, transactionally protected capacity, cancellation, ownership, relational rules | Verified by SQL-backed tests and browser booking/cancellation |
| 5 Database management | SQL schema/constraints, seed/update scripts, safe queries, CRUD | Verified with LocalDB; isolated tests preserve application data |
| 6 Security | JWT/RBAC, PBKDF2, request limits, random/private keys, account-owner MFA, generic recovery, local email capture, TLS ingress and trusted-proxy configuration | Local behavior verified; public TLS/domain/credentials are deployment prerequisites |
| 7 Testing | 26 backend, 5 frontend, 7 browser checks passed; one duplicate mobile auth case intentionally skipped | Verified locally; test inputs/assertions and results retained |
| 8 Performance and resilience | Cache, workers, metrics/logging/error handling, NGINX replicas, Redis, RabbitMQ, Prometheus/Grafana and smoke script | Native behavior verified; advanced runtime blocked by missing Docker/WSL |
| 9 Code standards and Git | Layered source, Git, ESLint/Prettier; Vite migration; package exclusions | Lint/build verified; final archive excludes generated/private files |
| 10 Execution and deployment | Relative startup scripts, generated private environment, initialization, Docker/Compose, Kubernetes, advanced CI/CD | Native verified; manifests render; container rollout/hosted pipeline not yet executed |
| 11 Submission materials | Source, schema, configuration, tests/evidence, README, report, presentation and packaging tool | Prepared; student must complete team details and upload archive |
| 12 Practical defense | PRESENTATION_GUIDE.md and executable demo commands | Prepared; attendance and individual understanding require students |
| 13 Assessment criteria | Evidence clearly distinguishes implementation from actual execution | No invented grade or completion percentage |
| 14 Integrity | References and factual AI assistance disclosure; TEAM_CONTRIBUTIONS.md | Real names/contributions/declaration need student confirmation |
| 15 Final provisions | 8 January 2027 deadline, one Moodle upload per group, all-member defense | Student responsibility; no upload performed |

The PDF's page 2 both mandates microservices and permits an appropriate modular monolith. This project supplies a notification service and queue integration; demonstrate them rather than relying on that wording conflict. Page 4 requires Kubernetes and advanced CI/CD. Configuration is implemented, but their runtime completion must remain pending until executed. The PDF permits appropriate alternative technologies, so justify ASP.NET Core/SQL Server during the defense.
