# Travel Agency

Travel Agency is a React and ASP.NET Core Web API project with JWT login, user dashboards, admin management, package booking, contact messages, and SQL Server storage.

## Project Structure

- `UI/travel-agency` - React frontend
- `Travel Agency Portal/Travel Agency Portal` - ASP.NET Core Web API
- `Microservices/NotificationService` - RabbitMQ-driven notification/MFA delivery microservice
- `Travel_AgencyDB.sql` - full SQL Server setup and seed script
- `Travel_Agency_Features_Update.sql` - safe update script for an existing database
- `Documentation` - report-ready documentation, requirement mapping, API testing notes, and submission support

## Requirements

- Node.js
- .NET SDK
- SQL Server
- SQL Server instance named `SHKELQIM`, or update `CRUDCS` in `appsettings.json`
- Docker Desktop, optional, if running with `docker compose`

## Database Setup

For a new database, run:

```text
Travel_AgencyDB.sql
```

For an existing database, run:

```text
Travel_Agency_Features_Update.sql
```

The database contains:

- `Users`
- `Places`
- `Hotels`
- `Travel_Packages`
- `Bookings`
- `Contact_Form`

Seed data includes:

- Admin user
- Paris, Bali, and Maldives destinations
- Example hotels
- Example travel packages

Default admin login:

```text
Email: admin@travelagency.com
Password: Admin123!
```

## Run Backend

From the API project folder:

```powershell
cd "C:\Users\Lenovo\OneDrive\Desktop\Travel Agency\Travel Agency Portal\Travel Agency Portal"
dotnet run --launch-profile http
```

Backend URL:

```text
http://localhost:5132
```

Swagger:

```text
http://localhost:5132/swagger
```

Health check:

```text
http://localhost:5132/health
```

## Run Frontend

From the React project folder:

```powershell
cd "C:\Users\Lenovo\OneDrive\Desktop\Travel Agency\UI\travel-agency"
npm start
```

Frontend URL:

```text
http://localhost:3000
```

## Main Features

- Register and login
- Forgot password reset with copy/paste reset code
- JWT session expiry set to 1 hour
- User dashboard with booking counts, upcoming booking, and package options
- User profile view/edit
- User password change from profile with current password verification
- Public destination catalog with search
- Public package catalog with filters
- Package details page
- User package booking
- User booking details and pending-booking cancellation
- Admin dashboard stats
- Admin destination management
- Admin hotel management
- Admin package management
- Admin booking management with customer info
- Admin user management with edit, role change, and delete
- Contact form
- Admin contact message read/archive/delete
- Demo MFA flow for protected roles such as admin
- Production-style MFA delivery through RabbitMQ and the notification microservice
- Redis-ready distributed caching with in-memory fallback
- RabbitMQ asynchronous processing
- Background cleanup and metrics snapshot workers
- Prometheus `/metrics` endpoint
- Audit log persistence through Entity Framework Core
- Logstash TCP log shipping into ELK
- Slack/webhook or email alerting hooks
- Optional MongoDB analytics integration endpoint
- Optional S3-compatible storage integration endpoint
- Kubernetes deployment manifests
- Automated SQL Server backup examples
- Playwright E2E browser tests

## API Versioning

All backend endpoints are exposed under:

```text
/api/v1
```

This keeps the current API version explicit and makes future versions easier to add.

## API Response Format

Successful API responses are wrapped consistently:

```json
{
  "success": true,
  "message": "Request completed successfully.",
  "data": {}
}
```

Validation and server errors use the same shape:

```json
{
  "success": false,
  "message": "Validation failed.",
  "data": {}
}
```

Unhandled backend errors are processed by global middleware and returned as clean JSON.

## Main API Endpoints

| Method | Endpoint | Auth | Purpose |
| --- | --- | --- | --- |
| `POST` | `/api/v1/auth/register` | Public | Register user |
| `POST` | `/api/v1/auth/login` | Public | Login and receive JWT |
| `POST` | `/api/v1/auth/forgot-password` | Public | Generate password reset code |
| `POST` | `/api/v1/auth/reset-password` | Public | Verify reset code and set new password |
| `GET` | `/api/v1/places` | Public | List/search destinations |
| `POST` | `/api/v1/places` | Admin | Add destination |
| `PUT` | `/api/v1/places/{id}` | Admin | Update destination |
| `DELETE` | `/api/v1/places/{id}` | Admin | Delete destination |
| `GET` | `/api/v1/hotels` | Public | List hotels |
| `POST` | `/api/v1/hotels` | Admin | Add hotel |
| `PUT` | `/api/v1/hotels/{id}` | Admin | Update hotel |
| `DELETE` | `/api/v1/hotels/{id}` | Admin | Delete hotel |
| `GET` | `/api/v1/packages` | Public | List/search packages |
| `GET` | `/api/v1/packages/{id}` | Public | Package details |
| `POST` | `/api/v1/packages` | Admin | Add package |
| `PUT` | `/api/v1/packages/{id}` | Admin | Update package |
| `DELETE` | `/api/v1/packages/{id}` | Admin | Delete package |
| `GET` | `/api/v1/bookings/mine` | User | User bookings |
| `GET` | `/api/v1/bookings/{id}` | User/Admin | Booking details |
| `POST` | `/api/v1/bookings` | User | Create booking |
| `PUT` | `/api/v1/bookings/{id}/cancel` | User | Cancel pending booking |
| `GET` | `/api/v1/bookings` | Admin | All bookings |
| `PUT` | `/api/v1/bookings/{id}/status` | Admin | Change booking status |
| `GET` | `/api/v1/users` | Admin | List users |
| `GET` | `/api/v1/users/{id}` | User/Admin | User profile |
| `PUT` | `/api/v1/users/{id}` | User/Admin | Update user |
| `PUT` | `/api/v1/users/{id}/password` | User | Change own password |
| `PUT` | `/api/v1/users/{id}/role` | Admin | Change role |
| `DELETE` | `/api/v1/users/{id}` | Admin | Delete user |
| `POST` | `/api/v1/contact` | Public/User | Send contact message to admin |
| `GET` | `/api/v1/contact` | Admin | List messages |
| `PUT` | `/api/v1/contact/{id}/read` | Admin | Mark message read |
| `PUT` | `/api/v1/contact/{id}/archive` | Admin | Archive message |
| `DELETE` | `/api/v1/contact/{id}` | Admin | Delete message |
| `GET` | `/api/v1/stats/admin` | Admin | Admin dashboard stats |

## API Infrastructure Notes

- Controllers expose REST endpoints.
- DTOs are used for API request models where input differs from database objects.
- A shared `DatabaseService` provides reusable ADO.NET database access.
- Entity Framework Core is also used for advanced support tables such as MFA codes, audit logs, and metrics snapshots.
- A separate notification microservice consumes RabbitMQ messages for MFA email delivery.
- JWT bearer authentication protects user/admin endpoints.
- Role-based authorization protects admin endpoints.
- Admin login supports an MFA verification step.
- Swagger/OpenAPI is enabled in development.
- CORS allows the React frontend to call the API from `localhost:3000`.
- `/health` exposes a simple service health endpoint.
- `/metrics` exposes Prometheus-compatible application metrics.
- Application logs can be shipped to Logstash when `Logging__Logstash__Host` is configured.
- API request logging records method, path, status code, and elapsed time through ASP.NET logging.
- Public read endpoints use short-lived response caching and distributed cache support with Redis fallback to memory cache.
- Login, password reset, and public contact writes are rate limited.
- Passwords are stored with salted PBKDF2 hashes; legacy SHA256 hashes are still accepted so existing seeded users continue to work.
- `Travel_Agency_Features_Update.sql` safely updates existing databases.

## Architecture Notes

This project is implemented as a monolithic ASP.NET Core REST API plus a React client. REST was selected instead of SOAP because the app mainly exposes JSON resources such as users, destinations, hotels, packages, bookings, and contact messages.

The repository now includes a demo-level enterprise infrastructure option with NGINX gateway/load balancing, Redis, MongoDB, MinIO, ELK, Prometheus, Grafana, and Alertmanager. The application still remains a modular monolith, but the repo now contains runnable support for those surrounding infrastructure requirements.

The repository also includes a notification microservice and RabbitMQ queue for asynchronous MFA delivery, plus Kubernetes manifests for a production-style deployment shape.

## Security Notes

For local development, `appsettings.json` contains development configuration. For production or Docker, override sensitive values with environment variables:

```text
Jwt__Secret
Jwt__Issuer
Jwt__Audience
ConnectionStrings__CRUDCS
```

Never commit real production secrets.

## Local Email Sending

Local Gmail SMTP settings are configured in:

```text
Travel Agency Portal/Travel Agency Portal/appsettings.Development.json
```

Current local email setup:

- Sender: `travel.agency@gmail.com`
- Test recipients: `shkelqim.maliqi@outlook.com`, `anisamustafa2020@gmail.com`
- SMTP host: `smtp.gmail.com`
- SMTP port: `587`

To actually send email, replace `PUT_GMAIL_APP_PASSWORD_HERE` with a Gmail App Password for `travel.agency@gmail.com`. Do not use the normal Gmail login password.

## Backend Tests

Run the API test project from the solution folder:

```powershell
cd "C:\Users\Lenovo\OneDrive\Desktop\Travel Agency\Travel Agency Portal"
dotnet test
```

The automated tests currently cover password hashing compatibility, the health endpoint, and authentication protection on secured endpoints.

## Docker

Run the API and SQL Server with Docker Compose:

```powershell
docker compose up --build
```

After SQL Server starts, apply `Travel_AgencyDB.sql` to create and seed the database. The API will be available at:

```text
http://localhost:5132
```

For the advanced requirement stack, use:

```powershell
docker compose -f docker-compose.enterprise.yml up --build
```

That compose file adds:

- NGINX API Gateway and load balancing
- Redis
- RabbitMQ
- Notification service
- MongoDB
- MinIO S3-compatible storage
- ELK stack
- Prometheus + Grafana + Alertmanager
- SQL Server backup helper

## Kubernetes

Kubernetes manifests are available at:

```text
Infrastructure/kubernetes
```

Apply them with:

```powershell
kubectl apply -k Infrastructure/kubernetes
```

Before using them in a real cluster, replace the example secret values in `secret.example.yaml`.

## CI/CD

The repository includes GitHub Actions workflows at:

```text
.github/workflows/backend-ci.yml
.github/workflows/frontend-ci.yml
```

They validate:

- ASP.NET Core restore, build, and backend tests
- React install, test, and production build

This satisfies the project's continuous testing requirement at coursework level, even though it does not yet include automated deployment to staging/production.

## Documentation Bundle

The repository now includes documentation files that directly support the PDF requirements:

- `Documentation/PROJECT_REPORT.md` - report-ready technical writeup
- `Documentation/REQUIREMENTS_MATRIX.md` - honest requirement-by-requirement status mapping
- `Documentation/ARCHITECTURE.md` - architecture explanation
- `Documentation/API_TESTS.md` - manual API demonstration checklist
- `Documentation/BACKUP_RECOVERY.md` - backup and recovery notes
- `Documentation/SUBMISSION_CHECKLIST.md` - final hand-in checklist
- `Documentation/ENTERPRISE_STACK.md` - advanced infrastructure overview
- `Documentation/SWAGGER_GUIDE.md` - OpenAPI/Swagger usage and demo walkthrough
- `CHANGELOG.md` - version/change communication support

If you need a `.docx` version for Moodle submission, run:

```powershell
python tools_create_report.py
```

## API Testing

Use Swagger first:

```text
http://localhost:5132/swagger
```

Extra manual testing notes are in:

```text
API_TESTS.md
```

## E2E Testing

Run browser tests from the frontend folder:

```powershell
cd "C:\Users\Lenovo\OneDrive\Desktop\Travel Agency\UI\travel-agency"
npx playwright install chromium
npm run e2e
```

## Useful Routes

- `/` - Home
- `/registerpage` - Register
- `/loginpage` - Login
- `/forgot-password` - Reset password
- `/dashboard` - User dashboard
- `/profile` - User profile settings
- `/destinations` - Public destination catalog
- `/packages` - Public package catalog
- `/packages/:id` - Package details
- `/my-bookings` - User bookings
- `/bookings/:id` - Booking details
- `/contactus` - Contact form
- `/admin` - Admin dashboard and management tools

## Notes

- Admin actions require a user with `U_Type = 'admin'`.
- Normal users cannot access `/admin`.
- Public users can view destinations and packages.
- Logged-in users can book packages, change their password from profile, and submit contact messages linked to their account.
- Users can cancel only pending bookings.
- Packages with zero seats are shown as sold out and cannot be booked.
- Registration and password reset require passwords with at least 8 characters, uppercase, lowercase, number, and special character.
