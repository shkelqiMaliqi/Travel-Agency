#!/usr/bin/env bash
set -euo pipefail

SQLCMD="/opt/mssql-tools/bin/sqlcmd"
if [ -x "/opt/mssql-tools18/bin/sqlcmd" ]; then
  SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
fi

HOST="${SQLSERVER_HOST:-sqlserver}"
DATABASE="${SQLSERVER_DATABASE:-Travel_Agency}"
USER="${SQLSERVER_USER:-sa}"
PASSWORD="${SQLSERVER_PASSWORD:-YourStrong!Passw0rd}"
INTERVAL="${BACKUP_INTERVAL_SECONDS:-86400}"
BACKUP_DIR="/backups/output"

mkdir -p "$BACKUP_DIR"

while true; do
  TS="$(date -u +%Y%m%dT%H%M%SZ)"
  FILE="/var/opt/mssql/backup/${DATABASE}_${TS}.bak"
  LOCAL_FILE="$BACKUP_DIR/${DATABASE}_${TS}.bak"

  echo "Starting SQL Server backup for ${DATABASE} at ${TS}"
  "$SQLCMD" -S "$HOST" -U "$USER" -P "$PASSWORD" -C -Q "BACKUP DATABASE [${DATABASE}] TO DISK = N'${FILE}' WITH INIT, COMPRESSION"
  echo "Backup command completed. SQL Server wrote ${FILE}."
  echo "Record this backup in backup logs. If SQL Server backup volume is mounted to ${BACKUP_DIR}, keep ${LOCAL_FILE}."

  sleep "$INTERVAL"
done
