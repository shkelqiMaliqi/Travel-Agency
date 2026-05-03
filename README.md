# Travel Agency

Travel Agency is a React and ASP.NET Core Web API project with JWT login, user dashboards, admin management, package booking, contact messages, and SQL Server storage.

## Project Structure

- `UI/travel-agency` - React frontend
- `Travel Agency Portal/Travel Agency Portal` - ASP.NET Core Web API
- `Travel_AgencyDB.sql` - full SQL Server setup and seed script
- `Travel_Agency_Features_Update.sql` - safe update script for an existing database

## Requirements

- Node.js
- .NET SDK
- SQL Server
- SQL Server instance named `SHKELQIM`, or update `CRUDCS` in `appsettings.json`

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
- Forgot password reset
- JWT session expiry set to 1 hour
- User dashboard with booking counts, upcoming booking, and package options
- User profile view/edit
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
| `POST` | `/api/v1/auth/forgot-password` | Public | Reset password |
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
| `PUT` | `/api/v1/users/{id}/role` | Admin | Change role |
| `DELETE` | `/api/v1/users/{id}` | Admin | Delete user |
| `POST` | `/api/v1/contact` | Public | Send contact message |
| `GET` | `/api/v1/contact` | Admin | List messages |
| `PUT` | `/api/v1/contact/{id}/read` | Admin | Mark message read |
| `PUT` | `/api/v1/contact/{id}/archive` | Admin | Archive message |
| `DELETE` | `/api/v1/contact/{id}` | Admin | Delete message |
| `GET` | `/api/v1/stats/admin` | Admin | Admin dashboard stats |

## API Infrastructure Notes

- Controllers expose REST endpoints.
- DTOs are used for API request models where input differs from database objects.
- A shared `DatabaseService` provides reusable ADO.NET database access.
- JWT bearer authentication protects user/admin endpoints.
- Role-based authorization protects admin endpoints.
- Swagger/OpenAPI is enabled in development.
- CORS allows the React frontend to call the API from `localhost:3000`.
- `Travel_Agency_Features_Update.sql` safely updates existing databases.

## API Testing

Use Swagger first:

```text
http://localhost:5132/swagger
```

Extra manual testing notes are in:

```text
API_TESTS.md
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
- Logged-in users can book packages and submit contact messages linked to their account.
- Users can cancel only pending bookings.
- Packages with zero seats are shown as sold out and cannot be booked.
