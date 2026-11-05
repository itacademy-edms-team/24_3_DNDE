#!/bin/bash
# Инициализация ролей и схем app_db (выполняется postgres-superuser при первом старте тома).
# app_runtime - только DML в схемах модулей, без DDL и без доступа к чужим схемам.
set -e

psql -v ON_ERROR_STOP=1 -U "$POSTGRES_USER" -d "$POSTGRES_DB" <<-SQL
    CREATE ROLE app_runtime LOGIN PASSWORD '${APP_DB_RUNTIME_PASSWORD}';
    CREATE ROLE accounts_owner LOGIN PASSWORD '${APP_DB_ACCOUNTS_OWNER_PASSWORD}';

    CREATE SCHEMA accounts AUTHORIZATION accounts_owner;

    GRANT CONNECT ON DATABASE ${POSTGRES_DB} TO app_runtime;
    GRANT USAGE ON SCHEMA accounts TO app_runtime;

    ALTER DEFAULT PRIVILEGES FOR ROLE accounts_owner IN SCHEMA accounts
        GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO app_runtime;
SQL
