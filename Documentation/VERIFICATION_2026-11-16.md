# Repair verification on 16 November 2026

The current project fixes the missing deployment pipeline, machine-specific setup, insecure shared signing-key default, misleading email behavior/recipient overrides, outdated frontend toolchain, loopback CORS, stale documentation, and source-package hygiene. User application data was preserved. The preceding requirements review remains a dated baseline, not the repaired status.

## Results actually obtained

| Check | Result |
| --- | --- |
| Backend Release tests | 26 passed; no failures/skips |
| Frontend Vitest | 5 tests passed in 3 suites |
| Frontend production build | Passed using Vite |
| ESLint | Passed; the last unused catch variable was removed |
| npm dependency install audit | Zero vulnerabilities; previously 73 |
| Browser public/mobile and full authenticated workflow | 7 passed, 1 intentionally skipped duplicate mobile full-auth case |
| Admin MFA and customer recovery | Real SMTP delivery to local-only Mailpit; no token/code exposed by UI/API |
| Booking and cancellation | Browser created a booking and cancelled it; available seats returned to five |
| Database isolation | Generated E2E database and owned temporary process trees cleaned up by harness |
| Kubernetes base render | Passed using checksum-verified kubectl v1.37.2 |
| Compose/Kubernetes runtime | Not run on this computer |
| GitHub-hosted CI/CD execution | Not performed; workflow definition added |

The authenticated browser scenario runs against a generated SQL database and temporary API/frontend ports. Public scenarios run at desktop and mobile sizes. The complete email/booking flow runs once on desktop to avoid competing challenges for the same seeded administrator. These results do not imply full coverage of every feature or a production security certification.

Machine-readable results: `Evidence/verification-2026-11-16.json`. Tests are in `Travel Agency Portal/Travel Agency Portal.Tests`, `UI/travel-agency/src/**/*.test.*`, and `UI/travel-agency/tests/e2e`. Exact commands are in README.md. Raw local diagnostics remain in ignored `Logs` and are omitted from the source archive.

## Infrastructure blocker

WSL and Docker Desktop were absent. The attempted WSL prerequisite installation was rejected by automatic approval review because it makes a persistent OS-level change and may require administrator access/a restart, while project repair was not considered explicit authorization for that host change. No workaround or installation was performed. Installing those prerequisites requires explicit user approval. After setup/restart, run the Compose smoke checks and Kubernetes rollout from DEPLOYMENT.md, then update this evidence with actual results.

## Deployment and academic conditions

The advanced workflow now exists and includes Compose integration tests, disposable kind rollout, optional registry publication, and optional staging deployment. No images were published and no remote cluster was contacted. Remote credentials, registry access, a public domain/certificate, and genuine SMTP remain environment-owned configuration. The local inbox avoids sending external email.

The prepared archive is a technical source package; it cannot certify originality or complete Moodle upload/attendance. Students must confirm names, actual contributions, references, AI assistance disclosure, and any personal declaration.

## Document format

The current technical report and presentation materials are delivered as Markdown. Word copies were regenerated from those sources, but visual rendering could not be completed: the bundled renderer lacks LibreOffice and the installed Word automation did not produce output. The unverified Word copies are excluded from the submission archive. The supplied six-page PDF does not mandate DOCX format. The portable report builder is included for a later Word export/review if desired.
