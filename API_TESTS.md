# API Test Notes

Use these checks in Swagger or Postman to demonstrate the Web API behavior.

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

6. `POST /api/v1/auth/forgot-password`
   - Body:

```json
{
  "email": "user@example.com"
}
```

   - Expected: returns a reset code for copy/paste testing.

7. `POST /api/v1/auth/reset-password`
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

3. `POST /api/v1/bookings`
   - Body:

```json
{
  "package_Id": 1,
  "travelers": 1
}
```

   - Expected: creates pending booking and reduces seats.

4. `GET /api/v1/bookings/mine`
   - Expected: returns current user's bookings.

5. `PUT /api/v1/bookings/{id}/cancel`
   - Expected: only pending bookings can be cancelled by user.

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
