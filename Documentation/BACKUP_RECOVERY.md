# Backup and recovery

Schema and seed scripts are versioned under `Database/`. `Travel_AgencyDB.sql` resets tables and is intended only for a newly created or explicitly disposable database. Do not run it over existing application data. `Travel_Agency_Features_Update.sql` adds absent feature structures/data and is used by the native launcher.

Before significant schema/data changes, obtain a SQL Server backup. The container examples include a backup helper and Kubernetes CronJob. They are configuration examples until a real backup/restore run is recorded. No restore over the user's application database was performed during repair.

For a deployed service, retain at least a current and previous full backup, store an encrypted off-machine copy, restrict backup access, and keep recovery credentials separately from source control. Choose retention/RPO/RTO targets appropriate to the environment. For the classroom environment, test a restore into a separately named disposable database and verify catalogs, accounts, and booking relationships before relying on the backup. Do not overwrite `Travel_Agency` during a drill.

Recovery: provision a separate target SQL database/server, restore the backup there, point a test API at it using a private environment connection string, and validate database-backed requests and authentication. After validation, plan a controlled switchover with the owner. `/health` alone is not proof that restored business data is correct.

Preserve `.env`/deployment secrets securely; do not include them in the source archive. Generate replacements only when creating a new environment or deliberately rotating credentials. Deleting local `.env` while retaining a database volume can make its SQL password inconsistent.
