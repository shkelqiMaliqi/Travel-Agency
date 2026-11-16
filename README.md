# Travel Agency

A React website and ASP.NET Core REST API for destinations, hotels, travel packages, bookings, profiles, and agency administration. SQL Server persists business data; JWT and RBAC protect user, admin, and auditor workflows. A separate notification service consumes RabbitMQ MFA messages.

## Verified development setup on Windows

Prerequisites: .NET 8 SDK/runtime, Node.js 22.12 or later, npm 11 recommended, SQL Server LocalDB (`MSSQLLocalDB`), and `sqlcmd` on PATH. Run commands from this repository root. No absolute paths for another computer are required.

```powershell
npm --prefix UI/travel-agency ci
dotnet restore 'Travel Agency Portal/Travel Agency Portal.sln'
./Tools/start-local.ps1
```

The startup script preserves an existing database, applies the additive feature update, generates private local configuration, downloads a pinned/checksum-verified Mailpit executable if needed, and starts hidden background services. Mailpit binds only to loopback and captures email locally; it does not deliver messages to real recipients. The one-time full database schema is run only if `Travel_Agency` is absent.

| Service | Address |
| --- | --- |
| Website | http://localhost:3000 |
| API documentation | http://localhost:5132/swagger |
| API health and metrics | http://localhost:5132/health and /metrics |
| Local MFA/recovery email inbox | http://localhost:8025 |

Development runs over HTTP on loopback. This is not the published HTTPS configuration. Development JWT keys are random, never the previously committed shared key. The launcher reads a persistent private key from `.env`; starting the API directly without one generates a process-local key, invalidating previous tokens on restart. Multiple API replicas must share a strong environment-provided key.

Seeded demo administrator: `admin@travelagency.com` / `Admin123!`; auditor: `auditor@travelagency.com` / `Auditor123!`. These are classroom accounts, not deployment credentials. Read the MFA code from the local inbox. Codes are never displayed by the website or returned by the API. Change/remove seeded credentials before any public deployment.

## Manual development commands

```powershell
dotnet run --project 'Travel Agency Portal/Travel Agency Portal/Travel Agency Portal.csproj' --launch-profile http
npm --prefix UI/travel-agency start
```

Start Mailpit first for MFA/recovery. The development connection targets `(localdb)\MSSQLLocalDB`. Override another SQL instance with `ConnectionStrings__CRUDCS`; set `Jwt__Secret` to a random value with at least 32 bytes for a persistent key. Base production settings contain no working credentials. Native startup uses SMTP at `127.0.0.1:1025`, with no recipient override.

## Automated verification

```powershell
dotnet test 'Travel Agency Portal/Travel Agency Portal.sln' -c Release
npm --prefix UI/travel-agency test
npm --prefix UI/travel-agency run lint
npm --prefix UI/travel-agency run build
cd UI/travel-agency
npx playwright install chromium
cd ../..
./Tools/verify-windows.ps1
```

Backend tests default to LocalDB. For a different SQL Server, set `TEST_SQLSERVER_CONNECTION`; the fixture creates/removes a randomly named test database and never resets `Travel_Agency`. The full browser harness creates its own `TravelAgencyE2E_*` database and temporary servers at ports 5152/3001, runs public/mobile and authenticated email/booking checks, then removes only its generated database and process trees. It requires the Mailpit inbox to be running. See `Documentation/VERIFICATION_2026-11-16.md` for actual results and limits.

## Docker Compose integration stack

Docker Desktop with WSL2/Linux containers is required on Windows. This machine did not have Docker/WSL at the time of repair; a container deployment is not claimed as verified here.

```powershell
./Tools/initialize-local.ps1
docker compose -f docker-compose.local.yml up -d --build --wait --wait-timeout 300
node Tools/verify-local-stack.cjs
```

This separate Compose project/SQL volume preserves its database across restarts. It runs two APIs, NGINX, Redis, RabbitMQ, the notification worker, Mailpit, SQL Server, Prometheus, Grafana, and a production frontend. URLs: frontend 3101, gateway 8088, inbox 8025, RabbitMQ management 15673, Prometheus 9091, Grafana 3002. Stop the native Mailpit first if port 8025 is occupied; do not run both email environments on the same ports. Random SQL/JWT/Grafana values are generated into ignored `.env` and never printed.

Stop the stack without deleting its database: `docker compose -f docker-compose.local.yml down`. Do not add volume-deletion flags when preserving data.

## Kubernetes and CI CD

`Infrastructure/kubernetes` supplies deployments, services, storage, initialization, HPA, frontend configuration, Mailpit, and a backup job. `Tools/start-kubernetes-local.ps1` restricts initialization to named local clusters and preserves existing secrets. A published deployment needs separately provisioned infrastructure and credentials; it must not use this local initializer.

```powershell
./Tools/initialize-local.ps1
./Tools/start-kubernetes-local.ps1
bash Tools/deploy-kubernetes.sh travel-agency-api:local travel-agency-notification-service:local travel-agency-frontend:local
```

Build/load these three images in the selected cluster first. See `Documentation/DEPLOYMENT.md`. The optional TLS ingress is separate from the base bundle; replace its example domain and provision a real certificate before applying it.

GitHub Actions includes backend CI with an explicit LocalDB test connection, frontend CI, browser CI, and `deploy-staging.yml`. The manually dispatched advanced pipeline tests Compose integrations, validates a real rollout in a disposable kind cluster, optionally publishes commit-tagged GHCR images, and optionally deploys to the GitHub `staging` environment. Publication and deployment default to false. Remote deployment needs `KUBE_CONFIG_BASE64`, registry pull access, and already provisioned namespace/configuration/secrets. No workflow dispatch, image publication, or remote deployment was performed during the local repair.

## Submission

Read `Documentation/REQUIREMENTS_MATRIX.md`, `SUBMISSION_CHECKLIST.md`, `PROJECT_REPORT.md`, and `PRESENTATION_GUIDE.md`. Add real team contributions and AI-use details in `TEAM_CONTRIBUTIONS.md`; these cannot be inferred or certified by the tooling. Build a source-only package using `python Tools/package-submission.py`. The archive excludes private environment files, generated dependencies/builds, logs, old backup copies, and historical reports with obsolete runtime claims. It includes current source, scripts, schema, test evidence, and documentation.

The supplied course PDF states 8 January 2027 as the deadline, one submission per group through Moodle, and attendance by every member at the practical defense. The user must perform these academic steps.
