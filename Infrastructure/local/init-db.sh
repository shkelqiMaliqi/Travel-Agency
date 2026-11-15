#!/usr/bin/env bash
set -euo pipefail
sqlcmd=/opt/mssql-tools18/bin/sqlcmd
exists=$($sqlcmd -S sqlserver -U sa -C -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name='Travel_Agency';" -b)
if [[ "${exists//[[:space:]]/}" == "0" ]]; then
  $sqlcmd -S sqlserver -U sa -C -i /schema.sql -b
  echo "Initialized the isolated local demo database."
else
  echo "Travel_Agency already exists; preserving its data."
fi
