#!/bin/bash

# Start SQL Server in the background
/opt/mssql/bin/sqlservr &

echo "Waiting for SQL Server to start..."

# Wait until SQL Server is ready
for i in {1..30}; do
    /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$SA_PASSWORD" -Q "SELECT 1" &> /dev/null
    if [ $? -eq 0 ]; then
        echo "SQL Server is up."
        break
    fi
    echo "SQL Server not ready yet... ($i)"
    sleep 2
done

echo "SQL Server is up. Running all .sql scripts in /scripts..."

# Ensure wildcard expansion doesn't fail if no .sql files exist
shopt -s nullglob

# Run init.sql first (if it exists)
if [[ -f "/scripts/sql/init.sql" ]]; then
    echo "Running: /scripts/sql/init.sql"
    sqlcmd -S localhost -U SA -P "$SA_PASSWORD" -d master -i "/scripts/sql/init.sql" -C
else
    echo "init.sql not found — skipping."
fi

shopt -s globstar
# Run all other .sql files
for script in /scripts/sql/**/*.sql; do
    if [[ "$script" != "/scripts/sql/init.sql" ]]; then
        echo "Running: $script in LocalDatabase"
        sqlcmd -S localhost -U SA -P "$SA_PASSWORD" -d LocalDatabase -i "$script" -C
    fi
done

echo "All scripts executed."

# Keep SQL Server running in the foreground
wait

