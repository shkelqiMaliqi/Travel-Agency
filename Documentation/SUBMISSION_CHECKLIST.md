# Submission Checklist

Use this checklist before final delivery to Moodle.

## Code and Project

- Backend builds successfully with `dotnet test`
- Frontend tests pass with `npm test -- --watchAll=false`
- Playwright E2E tests pass with `npm run e2e -- --project=chromium`
- Frontend builds successfully with `npm run build`
- Database script is present and up to date
- Docker files are present
- Notification microservice is present
- RabbitMQ queue integration is present
- Kubernetes manifests are present
- Automated backup examples are present
- CI workflows are present

## Documentation

- [`README.md`](C:/Users/Lenovo/OneDrive/Desktop/Travel%20Agency/README.md) is updated
- [`Documentation/PROJECT_REPORT.md`](C:/Users/Lenovo/OneDrive/Desktop/Travel%20Agency/Documentation/PROJECT_REPORT.md) is updated
- [`Documentation/REQUIREMENTS_MATRIX.md`](C:/Users/Lenovo/OneDrive/Desktop/Travel%20Agency/Documentation/REQUIREMENTS_MATRIX.md) matches the current repo honestly
- [`Documentation/API_TESTS.md`](C:/Users/Lenovo/OneDrive/Desktop/Travel%20Agency/Documentation/API_TESTS.md) is ready for demo use
- Backup/recovery notes are included
- Changelog is included

## Required Report Contents

- Executive summary
- Project goals and objectives
- Functional and non-functional analysis
- System design / architecture
- Implementation description
- Testing and results
- Conclusions and recommendations
- References
- Appendices
- Originality declaration

## Final Delivery Files

- `.docx` technical report
- project source archive `.zip` or `.rar`
- any supporting screenshots if required by the teacher

## Naming Reminder

Use the naming style requested by the PDF, such as:

- `EmriMbiemri_Projekti_SPDD.zip`
- `GrupiX_Projekti_SPDD.rar`

## Final Manual Checks

- Swagger opens
- health endpoint responds
- register/login works
- packages and destinations load
- admin access works
- contact form works
- RabbitMQ queue appears in enterprise stack
- Prometheus `/metrics` opens
- Kubernetes manifests are available in `Infrastructure/kubernetes`
