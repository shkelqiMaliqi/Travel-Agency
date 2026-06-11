# Backup and Recovery Notes

The PDF asks for backup and recovery policies for the database and critical configuration. This project now includes both a written backup policy and automated backup examples for Docker Compose and Kubernetes.

## Database Backup Policy

- Keep the schema and seed source under version control through:
  - [`Database/Travel_AgencyDB.sql`](C:/Users/Lenovo/OneDrive/Desktop/Travel%20Agency/Database/Travel_AgencyDB.sql)
  - [`Database/Travel_Agency_Features_Update.sql`](C:/Users/Lenovo/OneDrive/Desktop/Travel%20Agency/Database/Travel_Agency_Features_Update.sql)
- Before major schema changes, export a SQL Server backup or create a fresh `.bak`.
- For classroom/demo environments, keep at least:
  - one current full backup
  - one previous backup
  - one clean seed script snapshot
- Automated Docker backup helper:
  - [`Infrastructure/backups/sqlserver-backup.sh`](C:/Users/Lenovo/OneDrive/Desktop/Travel%20Agency/Infrastructure/backups/sqlserver-backup.sh)
- Automated Kubernetes backup helper:
  - [`Infrastructure/kubernetes/backup-cronjob.yaml`](C:/Users/Lenovo/OneDrive/Desktop/Travel%20Agency/Infrastructure/kubernetes/backup-cronjob.yaml)

## Configuration Backup Policy

Critical configuration values include:

- `Jwt__Secret`
- `Jwt__Issuer`
- `Jwt__Audience`
- `ConnectionStrings__CRUDCS`

Recommended practice:

- keep development defaults in `appsettings.json`
- override sensitive values in environment variables for Docker/production-like runs
- store production secrets outside the repository

## Recovery Procedure

### Option 1: Recreate from SQL scripts

1. Create or reset the SQL Server database.
2. Run `Database/Travel_AgencyDB.sql`.
3. Start the API.
4. Start the frontend.

### Option 2: Restore from database backup

1. Restore the latest SQL Server backup.
2. Confirm connection string values.
3. Start the API and validate `/health`.
4. Verify login and package endpoints.

## Recovery Validation Checklist

- `/health` returns success
- admin login works
- destinations load
- packages load
- bookings can be read
- contact message flow works

## Recommendation

For a more advanced production environment, add:

- retention rules
- off-machine backup storage
- restore drills
- infrastructure secret rotation
