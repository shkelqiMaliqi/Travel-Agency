# Travel Agency practical demonstration

Demonstrate only services actually running in the selected environment. The native website/API/email inbox and the advanced container stack are different environments; do not quote stale counts or claim all services are running merely because manifests exist.

## Opening explanation

Travel Agency lets customers browse destinations and packages and manage bookings. Administrators manage agency data; auditors have read-only access. The React frontend consumes an ASP.NET Core REST API. Controllers handle HTTP, services implement business rules, repositories persist parameterized SQL, and a separate notification service consumes RabbitMQ messages when that stack is enabled.

## Demonstration sequence

1. Start with `Tools/start-local.ps1`. Open the website and show database-backed destinations/packages.
2. In Swagger demonstrate `GET /api/v1/places`, `GET /api/v1/packages`, a missing item (404), a protected request without a token (401), and invalid registration (400). Explain JSON/status codes.
3. Log in as demo admin, read the MFA code from Mailpit, then show the admin workspace. Explain that a public deployment uses real email and different credentials.
4. Demonstrate create/update/delete of a temporary destination and explain that linked data prevents unsafe deletion. Use isolated automated tests for concurrency/booking boundary cases rather than modifying real records for a demonstration.
5. Show the browser harness running MFA, customer recovery, booking, and cancellation on a disposable database. Show 26 backend tests, five frontend tests, and the actual current browser result.
6. Explain JWT versus OAuth, RBAC/ownership, PBKDF2 hashing, parameterized SQL, validation, rate limiting, and the distinction between local HTTP and published TLS.
7. In a working container environment, show two gateway upstreams, Redis invalidation, RabbitMQ-to-worker-to-email delivery, and Prometheus/Grafana. Then show the Kubernetes rollout and advanced CI/CD artifacts. If not available, state the blocker and do not describe it as passed.
8. Each student explains their own contribution from `TEAM_CONTRIBUTIONS.md` and practices a small change such as adding a filter or validation.

## Questions to prepare for

- Why do you separate controllers, services, and repositories?
- How does the transaction prevent overselling and return seats exactly once on cancellation?
- Why are JWT and OAuth not the same concept?
- Where are secrets configured and what happens when the signing key is absent?
- Why are MFA/recovery tokens never displayed by the UI or API?
- What evidence distinguishes a configured service from a verified service?
- Which tests use isolated databases and which integrations need real infrastructure?

Do not memorize unverified runtime counts. Query the current system during the defense.
