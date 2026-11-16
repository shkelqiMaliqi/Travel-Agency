# Travel Agency requirements review

Reviewed: 16 November 2026.

Source: the six-page PDF supplied by the user, `C:\Users\Anisa Mustafa\Downloads\Kërkesat teknike_Ueb Shërbime  Ueb APIs 2026.pdf`.

Scope: source/configuration inspection, available documentation, the running development application, isolated SQL-backed backend tests, frontend tests, and browser/build checks. This is a coursework completion review, not a comprehensive security assessment. No application source fixes were made during this review.

## Verdict

The core application is implemented and demonstrably working. It is not yet accurate to mark every requirement complete. The largest gaps are demonstrable operation of the advanced infrastructure, the missing advanced deployment pipeline, secure environment configuration, and accurate submission evidence.

The supplied PDF differs from `../requirements_pdf_text.txt` and the source described by `../Documentation/REQUIREMENTS_MATRIX.md`. The existing matrix must be updated against this PDF before it is used as submission evidence. It currently marks many configured features as completed even when their live operation has not been verified here.

## Requirement-by-requirement assessment

“Implemented” means the source/configuration exists; “verified” means exercised during this review. A deployment manifest alone does not prove a running deployment.

| PDF section | Assessment | Evidence and outstanding work |
| --- | --- | --- |
| 1. Purpose and objectives | Implemented | `../Documentation/PROJECT_REPORT.md` describes the travel-agency problem, users, objectives, and real workflows. The website and database-backed API run locally. |
| 2. Problem analysis and requirements | Mostly complete | The report lists substantially more than five functional requirements, plus nonfunctional requirements. Roles are described in `../Documentation/ARCHITECTURE.md`. Add explicit reliability/maintainability targets and trace business processes/data sources to acceptance checks. |
| 3. Architecture and API design | Implemented; core verified | Controllers, application services, and repositories separate HTTP, business logic, and data access. Live OpenAPI 3.0.1 exposes 42 operations over 29 paths, exceeding the minimum of five endpoints. JSON, HTTP methods, validation, errors, and resource creation are present. A separate notification microservice exists, but its queue communication was not exercised here. |
| 4. Business logic | Implemented; representative cases verified | SQL-backed tests verify booking validation, concurrent seat reservation, cancellation, catalog relationships, registration, and recovery rules. Business logic resides in services/rules; SQL and transactions reside in repositories. |
| 5. Database management | Implemented; verified | SQL Server LocalDB contains destinations, hotels, packages, users, and support tables. Foreign keys/check constraints, parameterized SQL, initialization scripts, and CRUD exist. SQL-backed tests pass using a separate generated database. SQL Server is allowed by the PDF's allowance for other suitable technologies; explain the choice during the defense. |
| 6. API security | Partial | JWT, RBAC, salted PBKDF2 for new passwords, parameterized SQL, role/ownership restrictions, and rate limiting exist. Tests verify representative permissions and recovery protections. The application still ships a known development JWT signing key in base settings. Use environment configuration for genuine secrets; document local HTTP limits and configure HTTPS before publishing. Current SMTP settings are incomplete, preventing reliable privileged-role MFA and recovery email. |
| 7. API testing | Substantial coverage; further evidence needed | All 21 backend tests and 5 frontend tests pass in this review. Tests exercise CRUD, invalid data, permissions, concurrency, and recovery. Browser suite contains three public/form checks run at desktop/mobile sizes; it does not test the complete authenticated booking/admin/email workflow. Manual test documentation lists expected behavior but should include actual results and sample inputs from the final environment. |
| 8. Performance, resilience, monitoring | Partial | Live metrics, request logging, exception handling, background workers, and caching exist. Redis integration, NGINX balancing to two APIs, RabbitMQ messaging, and Prometheus/Grafana configuration are supplied. In the current local run Redis is blank (memory fallback), RabbitMQ is blank, and there is one API instance. Docker/kubectl are not available on PATH here. Historical performance/infrastructure JSON exists, but it was not independently reproduced. Run the infrastructure smoke script in the target environment and save current results. |
| 9. Code standards and version control | Mostly complete | Git history, layered code, README, ESLint, Prettier and editor configuration exist. Tracked runtime logs remain in the repository. Final packaging must exclude logs, dependencies, build outputs, private settings, and review scratch files. Do not claim a full Clean Code or security audit from this review. |
| 10. Execution and deployment | Partial; important gap | Frontend/API run locally after dependency installation and the LocalDB connection correction. Docker/Compose/Kubernetes configurations and `../Tools/deploy-kubernetes.sh` exist. Only backend, frontend, and E2E CI workflows are present. The README and verification report refer to `.github/workflows/deploy-staging.yml`, but that file is absent. No advanced image-publication/deployment pipeline is available in the current checkout. Kubernetes manifests were not run here. README also contains paths for a different computer and omits the LocalDB test override. |
| 11. Submission materials | Partial | Source, configuration, SQL scripts, tests, README, technical report, and a presentation guide exist. No final ZIP was found in the repository root. Update documents with actual results, clean the package, and make the archive required by the instructor. Moodle upload cannot be verified from local files. The current PDF does not repeat the older copy's mandatory DOCX/naming convention; use the attached requirements and any instructor clarification. |
| 12. Practical defense | Not verifiable from source | Prepare startup, five or more HTTP operations, database interaction, tests/results, architecture/security/error explanations, and individual contributions. The presentation guide exists but contains historical runtime counts and service/email claims that are not true of the currently running environment. Every group member must be able to explain and change the code. |
| 13. Assessment criteria | Not a separate implementation deliverable | The PDF evaluates real functionality, technical quality, accurate evidence, and individual defense. It gives a total of 30%, without a usable per-row weighting. No defensible completion percentage or grade can be calculated from this review. |
| 14. Academic integrity | Student action required | References exist. The generated technical report contains a placeholder originality declaration. Complete real authorship/contribution details, acknowledge reused resources, and transparently describe AI assistance according to course instructions. Personal understanding/originality cannot be verified by automated inspection. |
| 15. Final provisions and deadline | Student action required | The PDF states 8 January 2027 as the submission deadline, one upload per group, and mandatory participation by all group members in the defense. Moodle submission and attendance are not evidenced locally. |

## Important ambiguities in the PDF

- Page 2 says microservices are mandatory, then immediately permits a modular monolith when appropriate. Your notification service provides a real separate service boundary; demonstrate RabbitMQ communication rather than relying on that ambiguity.
- Page 3 explicitly requires caching, load balancing, message queues, and monitoring to be included. Their source/configuration exists, but their full current operation remains unverified.
- Page 4 says Docker/Compose are recommended and Kubernetes plus advanced CI/CD must be used. Treat Kubernetes and the missing deployment workflow as outstanding until demonstrated or clarified by the instructor.
- Pages 2 and 5 permit suitable alternative technologies when justified. ASP.NET Core and SQL Server are not automatic failures under this supplied PDF.

## Verification performed

Backend command:

```powershell
$env:TEST_SQLSERVER_CONNECTION='Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true'
dotnet test 'Travel Agency Portal.Tests\Travel Agency Portal.Tests.csproj' --verbosity minimal
```

Result: **21 passed, 0 failed, 0 skipped**. `SqlApiFixture` creates a randomly named `TravelAgencyTests_*` database, loads the schema into it, and removes that test database afterward. It does not initialize or clear the application's `Travel_Agency` database. Recovery delivery is captured by a test double, Redis is disabled, and MFA-required roles are disabled for the SQL fixture. Therefore these tests do not prove live SMTP, RabbitMQ, Redis, or the complete MFA login flow.

Frontend command (from `../UI/travel-agency`):

```powershell
$env:CI='true'
npm test -- --watchAll=false --runInBand
```

Result: **5 tests passed across 3 suites**. Dependency/deprecation warnings were reported.

Browser test prerequisites: the initial run could not start because the Playwright Chromium binary was absent. After `npx playwright install chromium`, the rerun passed **6 of 6 tests** (three scenarios across desktop and mobile Chromium) in 12.2 seconds:

```powershell
$env:E2E_BASE_URL='http://127.0.0.1:3000'
npx playwright test --reporter=line
```

`npm run lint` completed without reported lint errors. `npm run build` compiled successfully; the gzip main JavaScript bundle was approximately 59 KB. The build reports old Browserslist data and a missing declared Babel dependency in the Create React App toolchain. Passing the build does not resolve the dependency vulnerabilities reported during installation (73 findings: 19 low, 18 moderate, 32 high, 4 critical). Those counts concern the installed dependency tree and require triage; they are not proof that all findings are exploitable in the deployed site.

Live HTTP checks:

| Request | Expected | Observed | One sample elapsed time |
| --- | --- | --- | --- |
| `GET /health` | 200 | 200 | 753.0 ms |
| `GET /api/v1/places` | 200 with stored destinations | 200; 17 destinations | 85.3 ms |
| `GET /api/v1/packages` | 200 with stored packages | 200; 17 packages | 105.7 ms |
| `GET /api/v1/hotels` | 200 with stored hotels | 200; 17 hotels | 78.5 ms |
| `GET /api/v1/places/2147483647` | 404 | 404 | 107.0 ms |
| `GET /api/v1/users` without credentials | 401 | 401 | 18.1 ms |
| `GET /metrics` | 200 | 200 | 340.9 ms |
| `GET /swagger/v1/swagger.json` | 200 with API description | 200; OpenAPI 3.0.1, 42 operations | 1754.1 ms |

These are single local request measurements including client/tool overhead, not load tests or statistically reliable latency estimates. Historical before/after browser timings are available in `../Documentation/performance-*.json`; their past execution is not independently confirmed by their presence.

## Prioritized remaining work

1. Supply the missing advanced deployment CI/CD workflow, or correct the README/report claim if it was never implemented. Verify SQL integration tests in CI with an explicit reachable test SQL Server configuration; the current backend workflow does not set `TEST_SQLSERVER_CONNECTION` and the test fixture otherwise reads the machine-specific base setting.
2. Run and demonstrate Kubernetes, the two-instance gateway, Redis, RabbitMQ/notification service, and Prometheus/Grafana. Save actual command output and fresh smoke-check evidence. Historical `infrastructure-verification.json` is useful prior evidence, not proof of the current machine's state.
3. Configure safe local email capture or genuine SMTP for the intended environment, then verify admin/auditor MFA and password recovery end to end. Do not expose MFA codes or disable MFA to make the demonstration work. Current development recipient overrides also mean MFA may be addressed to demo recipients rather than the account owner.
4. Move genuine secrets to environment variables/secret storage, replace the known signing key for any published deployment, and configure/document HTTPS there. The current Kubernetes ingress has no TLS section.
5. Correct installation/test instructions and synchronize the requirements matrix, technical report, presentation guide, service URLs, results, and contribution/AI-use statements with the actual final version.
6. Produce a clean source archive and submit through Moodle. Exclude `node_modules`, `bin`, `obj`, generated builds, logs, local secrets, and temporary review outputs. Keep the SQL schema, safe update scripts, configuration examples, test source/results, and presentation materials.

No deployment, external email, Moodle upload, source cleanup, or feature fixes were performed as part of this assessment.
