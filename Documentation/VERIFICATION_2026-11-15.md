# Requirements fixes and verification — 15 November 2026

This report distinguishes implementation from live verification. The course PDF is the requirements source; deployment examples alone are not treated as proof of operation.

## Changes

| Area | Implementation |
| --- | --- |
| Password recovery | Public responses never include a recovery token and are identical for existing and unknown accounts. SMTP delivers the token to the account owner's address. Tokens use 256 bits of cryptographic randomness, expire after 15 minutes, are stored as hashes, and are consumed once inside a SQL transaction. Requests are limited to three per IP per 15 minutes, in addition to the authentication endpoint limits. |
| Architecture | HTTP controllers contain routing, authorization, binding, and result translation. Application services coordinate business rules. Typed repositories contain parameterized SQL, row mapping, and transaction handling. Booking rules are explicit and tested. |
| Cache correctness | Catalog lists and details share versioned Redis keys. Changes to destinations, hotels, packages, and bookings invalidate all filtered/detail variants across API instances. Cache failure falls back to SQL. Entries expire after 20 seconds. |
| API responses | Catalog and booking creation return 201 with a Location header and the new resource ID. Validation, missing records, ownership restrictions, and role restrictions are tested. |
| Delivery reliability | RabbitMQ publishing waits for confirmation. Notification retries are bounded and failed deliveries go to a separate queue; expired MFA messages are discarded. MFA generation uses a cryptographic random generator and verification consumes challenges atomically. The public test-email endpoint was removed. |
| Frontend performance | Route code splitting, intent-based prefetching, GET deduplication, safe cache invalidation after mutations/account changes, responsive image sizing, local image fallbacks, and removal of the duplicate external Bootstrap stylesheet. Public GET requests omit the unnecessary JSON Content-Type header, avoiding its CORS preflight. Private responses are fetched fresh. |
| Production serving | NGINX serves the React production build with gzip, year-long immutable caching for hashed assets, uncached HTML, SPA fallback routes, and a same-origin API proxy. The API also supports response compression. |
| Git cleanup | 692 generated files were removed from the index with `git rm --cached`; working copies remain on disk. Ignore rules cover generated builds, dependencies, editor state, logs, test results, backups, and local environment files. Docker build context excludes these files and local secrets. Existing source/configuration edits were preserved. |

## Executable verification

The SQL tests create `TravelAgencyTests_<random identifier>`, load the repository schema, and remove that database afterward. They never initialize, clear, or drop the developer's application database.

```powershell
dotnet test "Travel Agency Portal/Travel Agency Portal.sln"
cd UI/travel-agency
npm test -- --watchAll=false
$env:E2E_BASE_URL='http://127.0.0.1:3101'
npx playwright test
```

Backend tests cover destination/hotel/package CRUD, relational validation, JWT roles, ownership, structured validation responses, concurrent seat reservation, cancellation rules, cache invalidation, token hashing/replay/expiry, registration role protection, password login, and recovery rate limiting.

Frontend tests cover public request deduplication, avoiding unnecessary preflight headers, the race between a mutation and an in-flight read, private-data isolation, the recovery form, and application rendering. Browser tests cover desktop and mobile public navigation and authentication forms.

Raw backend test output is written to `Logs/TestResults/verification.trx` and is intentionally ignored by Git. The shareable infrastructure evidence is `Documentation/infrastructure-verification.json`.

Verified results: 21 backend tests, 5 frontend tests, 6 desktop/mobile browser tests, and all 6 infrastructure smoke checks passed. The application deployment script also completed all three rollouts and its two HTTP smoke requests against the local Docker Desktop Kubernetes cluster. GitHub-hosted execution and remote staging credentials have not been exercised.

## Local infrastructure

```powershell
docker compose -f docker-compose.local.yml up -d --build
node Tools/verify-local-stack.cjs
```

This is a separate Compose project and SQL volume. The initialization job creates its database only when absent and preserves it on subsequent starts. It uses a local mail inbox, so verification never sends real email. Its credentials are development examples and must be replaced for hosting.

| Service | Local URL |
| --- | --- |
| Production frontend | http://localhost:3101 |
| API / Swagger | http://localhost:8088/swagger |
| Mailpit test inbox | http://localhost:8025 |
| RabbitMQ management | http://localhost:15673 |
| Prometheus | http://localhost:9091 |
| Grafana | http://localhost:3002 |

The smoke script verifies two distinct API upstreams, RabbitMQ-to-worker-to-inbox MFA delivery, cross-instance cache invalidation, real password recovery and replay rejection, both Prometheus scrape targets, compressed immutable assets, SPA routing, and the same-origin API proxy. It deletes its temporary catalog/user records. Test inbox messages remain available for manual inspection.

Stop this stack without deleting its database:

```powershell
docker compose -f docker-compose.local.yml down
```

## Deployment

`.github/workflows/deploy-staging.yml` is a manually dispatched pipeline with SQL integration tests, frontend tests/build/browser checks, three GHCR images tagged with the source commit, and an optional staging deployment. Image publication uses the repository's GitHub token.

`Tools/deploy-kubernetes.sh` applies only application deployments, services, and the frontend config. It waits for rollouts and checks the frontend and a database-backed API request. It does not initialize databases, replace storage, or apply example credentials. Failed rollouts stop the pipeline; Kubernetes rollout history remains available for diagnosis or rollback.

Remote staging requires a GitHub `staging` environment with `KUBE_CONFIG_BASE64`, a reachable cluster with its namespace/configuration/credentials and database already provisioned, and permission for the cluster to pull the GHCR images. Configure an image-pull secret on its service account when the packages are private. Dispatch without `deploy` to test/build/publish only; enable `deploy` to perform the rollout. Configure environment reviewers in GitHub if required by your team.

The local Docker Desktop Kubernetes context can run the same deployment script using the locally built `:local` images. This validates the deployment mechanics separately from GitHub's remote credentials and registry permissions.

## Performance measurements and limits

`Tools/measure-pages.cjs` uses fresh Playwright browser contexts against the same local production preview (`node Tools/preview.cjs`, port 3100). Raw before/after observations are stored in the adjacent JSON files. Run it after `npm run build`:

```powershell
node Tools/measure-pages.cjs Documentation/performance-after.json
```

The initial compressed JavaScript bundle fell from 73,310 bytes to about 59 KB (roughly 19% less). The admin and other route chunks are downloaded when needed. The external Bootstrap CSS request was removed. Page timing observations are single local samples, affected by browser warm-up, external image/network availability, and Docker load; they are evidence of the test conditions, not a production speed guarantee. No sustained-load claim is made.

Production hosting still requires genuine SMTP credentials, TLS/domain/ingress configuration, managed secrets, registry access, and environment-specific monitoring/backup operation. The existing base Kubernetes bundle still contains example infrastructure credentials and should not be applied unchanged to production. The staging application deployment script deliberately avoids that bundle.
