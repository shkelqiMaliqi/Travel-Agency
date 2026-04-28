# Travel Agency

Travel Agency is a React and ASP.NET Core project with JWT login, user dashboards, admin destination management, contact messages, and SQL Server storage.

## Project Structure

- `UI/travel-agency` - React frontend
- `Travel Agency Portal/Travel Agency Portal` - ASP.NET Core Web API
- `Travel_AgencyDB.sql` - SQL Server database setup and seed data

## Requirements

- Node.js
- .NET SDK
- SQL Server
- SQL Server instance named `SHKELQIM`, or update `CRUDCS` in `appsettings.json`

## Database Setup

Run `Travel_AgencyDB.sql` in SQL Server Management Studio.

The script creates:

- `Users`
- `Places`
- `Contact_Form`

It also seeds:

- Admin user
- Paris, Bali, and Maldives destinations

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
- JWT session expiry set to 1 hour
- User dashboard
- User profile view/edit
- Admin dashboard
- Admin add/edit/delete destinations
- Public destinations page
- Contact form

## Useful Routes

- `/` - Home
- `/registerpage` - Register
- `/loginpage` - Login
- `/dashboard` - User dashboard
- `/profile` - User profile settings
- `/admin` - Admin destination management
- `/destinations` - Public destination catalog
- `/contactus` - Contact form

## Notes

- Admin actions require a user with `U_Type = 'admin'`.
- Normal users cannot access `/admin`.
- Public users can view destinations and submit contact messages.
