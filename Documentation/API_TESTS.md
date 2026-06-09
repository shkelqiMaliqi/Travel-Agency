# API Test Notes

Use these checks in Swagger or Postman to demonstrate the Web API behavior.

Swagger usage notes and the recommended presentation flow are also documented in `Documentation/SWAGGER_GUIDE.md`.

## Public Requests

0. `GET /health`
   - Expected: returns `Healthy`.

1. `GET /api/v1/places`
   - Expected: returns destinations in a standard API response wrapper.

2. `GET /api/v1/packages`
   - Expected: returns packages with destination and hotel names.

3. `GET /api/v1/packages?search=Maldives`
   - Expected: returns the Maldives package.

4. `POST /api/v1/auth/register`
   - Expected: creates a user and returns a JWT response.

5. `POST /api/v1/auth/login`
   - Expected: returns token, role, user id, name, and email.
   - Admin/demo MFA behavior: if MFA is required for the role, the response returns `requiresMfa = true` and a demo verification code.

6. `POST /api/v1/auth/verify-mfa`
   - Body:

```json
{
  "email": "admin@travelagency.com",
  "code": "123456"
}
```

   - Expected: verifies the one-time MFA code and returns the final JWT token.

7. `POST /api/v1/auth/forgot-password`
   - Body:

```json
{
  "email": "user@example.com"
}
```

   - Expected: returns a reset code for copy/paste testing.

8. `POST /api/v1/auth/reset-password`
   - Body:

```json
{
  "email": "user@example.com",
  "resetCode": "123456",
  "newPassword": "NewPass123!",
  "confirmPassword": "NewPass123!"
}
```

   - Expected: verifies code and updates the password.

## Authenticated User Requests

Add the JWT token in Postman/Swagger:

```text
Authorization: Bearer <token>
```

1. `GET /api/v1/users/{id}`
   - Expected: user can read own profile.

2. `PUT /api/v1/users/{id}`
   - Expected: user can update own profile.

3. `PUT /api/v1/users/{id}/password`
   - Body:

```json
{
  "currentPassword": "OldPass123!",
  "newPassword": "NewPass123!",
  "confirmPassword": "NewPass123!"
}
```

   - Expected: user can change own password after current password is verified.
   - Validation: new password must match the password policy and confirmation.

4. `POST /api/v1/bookings`
   - Body:

```json
{
  "package_Id": 1,
  "travelers": 1
}
```

   - Expected: creates pending booking and reduces seats.

5. `GET /api/v1/bookings/mine`
   - Expected: returns current user's bookings.

6. `PUT /api/v1/bookings/{id}/cancel`
   - Expected: only pending bookings can be cancelled by user.

7. `POST /api/v1/contact`
   - Expected: creates an unread message for the admin Messages tab.
   - Logged-in users should have the message linked through their JWT user id.

## Admin Requests

Login with:

```text
admin@travelagency.com
Admin123!
```

Use the admin JWT token.

1. `POST /api/v1/places`
   - Expected: admin can create destination.

2. `POST /api/v1/hotels`
   - Expected: admin can create hotel linked to destination.

3. `POST /api/v1/packages`
   - Expected: admin can create package.
   - Validation: hotel must belong to selected destination.

4. `GET /api/v1/bookings`
   - Expected: admin sees all bookings with customer info.

5. `PUT /api/v1/bookings/{id}/status`
   - Expected: admin can set `Pending`, `Confirmed`, or `Cancelled`.

6. `GET /api/v1/users`
   - Expected: admin sees all users.

7. `PUT /api/v1/users/{id}/role`
   - Expected: admin can change role.
   - Validation: admin cannot remove own admin access.

8. `GET /api/v1/contact`
   - Expected: admin sees unarchived messages.

9. `GET /api/v1/stats/admin`
   - Expected: returns users, bookings, pending bookings, packages, sold out packages, and revenue.

10. `GET /api/v1/infrastructure/audit-logs`
   - Expected: returns recent audit entries stored through Entity Framework Core.

11. `GET /api/v1/infrastructure/metrics-snapshots`
   - Expected: returns periodic background-worker metrics snapshots.

12. `POST /api/v1/infrastructure/analytics-events`
   - Expected: stores a demo analytics event in MongoDB if Mongo is configured.

13. `POST /api/v1/infrastructure/storage-demo`
   - Expected: uploads a text payload to S3-compatible storage if MinIO/S3 config is available.

## Error Tests

1. Call an admin endpoint without a token.
   - Expected: `401 Unauthorized`.

2. Call an admin endpoint with a normal user token.
   - Expected: `403 Forbidden`.

3. Create a package with an end date before start date.
   - Expected: validation error response.

4. Try to book more travelers than available seats.
   - Expected: error message about unavailable seats.

5. Send repeated login or forgot-password requests quickly.
   - Expected: API eventually returns `429 Too Many Requests`.

6. Visit `/metrics`.
   - Expected: Prometheus-compatible metrics output is returned.

## Infrastructure Demonstrations

1. Run the enterprise stack:

```powershell
docker compose -f docker-compose.enterprise.yml up --build
```

   - Expected: NGINX gateway, Redis, RabbitMQ, notification service, SQL Server, MongoDB, MinIO, ELK, Prometheus, Grafana, Alertmanager, and backup helper services are defined.

2. Open RabbitMQ management:

```text
http://localhost:15672
```

   - Expected: the `travel-agency.mfa.delivery` queue can receive MFA delivery messages when admin MFA is triggered.

3. Open Kibana:

```text
http://localhost:5601
```

   - Expected: Logstash receives JSON log lines from the API when `Logging__Logstash__Host=logstash`.

4. Open Prometheus and Alertmanager:

```text
http://localhost:9090
http://localhost:9093
```

   - Expected: Prometheus scrapes `/metrics` and Alertmanager is configured for email/webhook alert routing.

5. Run E2E tests:

```powershell
cd "UI/travel-agency"
npx playwright install chromium
npm run e2e
```

   - Expected: browser tests verify public navigation and authentication form rendering.
