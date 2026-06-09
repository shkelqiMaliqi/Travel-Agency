# Swagger and OpenAPI Guide

Swagger UI is available at:

- `http://localhost:5132/swagger`

The API documentation now includes:

- OpenAPI v3 document generation
- bearer token authentication support
- request body examples for authentication and infrastructure demo endpoints
- response examples for login, MFA verification, places, packages, analytics, and storage endpoints
- common error response descriptions for `400`, `401`, `403`, `429`, and `500`

## Recommended Demo Flow

1. Open Swagger UI.
2. Call `POST /api/v1/auth/login` with the seeded admin account.
3. If the response returns `requiresMfa = true`, copy the demo code.
4. Call `POST /api/v1/auth/verify-mfa`.
5. Use the returned JWT in the `Authorize` button.
6. Test admin endpoints such as:
   - `GET /api/v1/users`
   - `GET /api/v1/infrastructure/audit-logs`
   - `GET /api/v1/infrastructure/metrics-snapshots`
7. Test public endpoints such as:
   - `GET /api/v1/places`
   - `GET /api/v1/packages`
8. Visit `GET /metrics` separately to show Prometheus output.

## Why This Matters for the Requirements

This satisfies the documentation-related requirement by providing:

- endpoint descriptions
- example requests
- example responses
- security scheme definition
- interactive testing directly from the browser
