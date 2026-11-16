# Travel Agency technical report

## Executive summary

Travel Agency implements a functional travel booking and administration platform with React, ASP.NET Core, and SQL Server. Its core workflows are verified locally. The repository includes notification messaging, distributed caching, gateway/load-balancing, monitoring, Kubernetes, and an advanced CI/CD pipeline; operation of those infrastructure components must be supported by actual target-environment evidence. This report separates implementation from verified execution.

## Problem and objectives

Customers need a single place to discover trips, reserve available capacity, review bookings, and manage account information. Staff need controlled administration of destinations, hotels, packages, bookings, users, and contact messages. Auditors need read-only access. Objectives are a usable website, consistent REST resources, persistent relational data, protected role-based operations, and reproducible tests/deployment.

## Requirements and acceptance

Functional requirements include registration/login, privileged-role MFA, password recovery, catalog browsing/filtering, booking/cancellation, profile management, contact messages, and administration. These exceed the five functional requirements in the supplied PDF. Sources are user input and the SQL schema/seed scripts. Main business processes are account authentication/recovery, catalog maintenance, seat reservation/cancellation, and contact handling.

Nonfunctional acceptance criteria: unauthorized requests return 401/403; invalid models return 400; concurrent bookings cannot oversell; cancellation restores seats once; unexpected exceptions return a structured response without terminating the app; test runs preserve the application database; repeat catalog reads use a 20-second cache with mutation invalidation; secrets are supplied privately for deployments. Reliability and maintainability are supported by explicit layers, automated tests, startup diagnostics, and reproducible configuration. Representative response times are measured and saved rather than asserted as universal performance guarantees.

## Architecture and data model

React sends JSON HTTP requests to versioned `/api/v1` controllers. Application services coordinate domain rules and repositories execute parameterized SQL and transactions. EF Core manages support data. `Users`, `Places`, `Hotels`, `Travel_Packages`, `Bookings`, and `Contact_Form` are the core relational entities. Packages reference a place and hotel; bookings reference a package and account. Foreign keys/check constraints preserve relational validity. MFA, reset tokens, audit events, and metrics snapshots have separate tables.

The independent NotificationService consumes durable RabbitMQ messages and sends account email. Redis provides distributed caching when configured; otherwise native development uses memory. NGINX balances Compose requests across two API instances. Prometheus scrapes API metrics and Grafana visualizes them. Kubernetes resources describe orchestration, persistence, startup/readiness, HPA, and backup jobs.

## Implementation and technology decisions

REST/JSON matches a resource-oriented browser application. URL versioning provides clear contracts. SQL Server is technically appropriate for relational bookings and transactionally consistent seat management; the PDF permits other suitable database technologies when justified. ASP.NET Core provides routing, validation, dependency injection, JWT middleware, and hosted workers. The frontend uses React 18, React Router 7, and Vite; tests use xUnit, Vitest, and Playwright. Exact resolved frontend versions are in package-lock.json and backend versions in the project files.

## Security and error handling

JWT is a token format used here for bearer authentication, not an OAuth authorization flow. RBAC and ownership checks restrict operations. New passwords use salted PBKDF2; legacy seed hashes remain for classroom compatibility and should be changed before publishing. Parameterized SQL and bounded request models reduce injection/truncation risks. MFA and recovery secrets are hashed in storage and never returned to callers. MFA goes to the account owner, with delivery failure returning 503 rather than a false success. Recovery responses conceal account existence.

Base settings contain no shared signing key or working production database credential. Direct development startup generates a random process-local signing key; private environment configuration supplies a persistent key for replicas. Known example/short/missing production signing keys are rejected at startup. Native email is captured only by loopback Mailpit. Published environments require genuine SMTP, TLS certificates, secret storage, and explicitly trusted ingress IPs. Local HTTP is a documented development limitation. Dependencies were updated and the current npm audit reports zero findings; this is not a comprehensive security certification.

## Testing and results

The 16 November repair run verifies 26 backend cases, including production-key rejection and generated development keys, plus CRUD, role/ownership protection, validation, booking concurrency/cancellation, and recovery expiry/replay protections. Five frontend tests pass; lint and production build pass after migration away from Create React App. The full browser harness uses a generated database and local email capture to exercise privileged MFA, customer recovery, booking, and cancellation along with public/mobile pages. Its exact result and infrastructure limits are recorded in `VERIFICATION_2026-11-16.md`.

The SQL test fixture and browser harness create their own random databases and clean up only those databases. Application data is preserved. The API exposes 42 documented operations over 29 paths. Existing historical measurements are not presented as results of the repaired version; fresh evidence belongs in `Documentation/Evidence`.

## Deployment and reproducibility

The root README provides portable relative startup/test commands. `Tools/start-local.ps1` generates private local configuration and preserves existing data. Compose uses generated secrets, two API replicas, Redis, RabbitMQ/worker, local email, monitoring, and production frontend serving. The advanced manually dispatched workflow tests this stack, rolls out to a disposable kind cluster, and optionally publishes/deploys commit-tagged images through a configured GitHub staging environment. It has not been run on GitHub as part of the local repair.

The Kubernetes base omits automatic example secrets/ingress. Local initialization is restricted to named local contexts. Published TLS ingress is a separate configuration requiring a real domain/certificate. Container verification on this computer requires installing Docker/WSL and a cluster; absence of that infrastructure cannot be solved by changing a manifest or claiming a rollout passed.

## Conclusions and remaining conditions

The core application and automated quality checks work locally. Remaining external conditions are infrastructure installation/execution, hosted workflow verification, any actual public environment credentials/domain/certificate, and the students' academic submission/defense. The requirements matrix records these conditions honestly. Students must complete their real contributions and disclosures before submission and explain the implementation individually.

## References

- Supplied course PDF: Kërkesat teknike Ueb Shërbime Ueb APIs 2026, six pages, Prof. Liridon Hoti.
- ASP.NET Core documentation: https://learn.microsoft.com/aspnet/core/
- SQL Server documentation: https://learn.microsoft.com/sql/
- React documentation: https://react.dev/
- Vite documentation: https://vite.dev/guide/
- GitHub Actions container publication: https://docs.github.com/actions/tutorials/publish-packages/publish-docker-images
- Mailpit installation: https://mailpit.axllent.org/docs/install/
- Kubernetes documentation: https://kubernetes.io/docs/

## Authorship and AI assistance

Real team membership and contributions must be supplied by the students in `TEAM_CONTRIBUTIONS.md`. AI assistance was used for review, repairs, testing, and documentation. Each student must review and understand the final work and make any originality declaration personally under the course's rules.
