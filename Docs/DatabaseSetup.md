# Database Setup Guide

This template can run without a database. Persistence, local Identity, and database
migrations are enabled only when the `Database` capability is explicitly configured.

Supported providers:

- PostgreSQL, recommended for new applications.
- MySQL or MariaDB, retained as an optional provider.

Only one provider is active for an application instance. PostgreSQL and MySQL have
independent migration histories and must never share migration files.

## Architecture at a Glance

```text
AselDevBlazorArchitecture.Infrastructure
  AppDbContext, Identity, persistence registration, provider selection

AselDevBlazorArchitecture.Migrations.PostgreSql
  PostgreSQL-only migrations and design-time factory

AselDevBlazorArchitecture.Migrations.MySql
  MySQL-only migrations and design-time factory

AselDevBlazorArchitecture.Web
  Configuration and application host; never applies migrations at startup
```

The two migration projects are intentionally small provider adapters. They are not
business layers and must not contain entities, services, repositories, or application
logic. Keeping them separate prevents PostgreSQL annotations and SQL from entering the
MySQL history, and vice versa.

## 1. Prerequisites

Install:

- .NET 8 SDK.
- PostgreSQL 14 or newer, or access to a PostgreSQL server.
- MySQL 8 or newer when using the optional MySQL provider.

Restore the repository-local EF Core tool:

```powershell
dotnet tool restore
```

The tool version is pinned in `.config/dotnet-tools.json`. A global `dotnet-ef`
installation is not required.

## 2. Default No-Database Mode

The committed configuration is safe and contains no credentials:

```json
"Database": {
  "Enabled": false,
  "Provider": "postgresql",
  "ConnectionString": ""
}
```

In this mode:

- The web application starts normally.
- No database connection is attempted.
- No migration or seed operation runs.
- Database-backed sign-in and user administration return an unavailable response.

## 3. Store Local Secrets

Never place real credentials or JWT signing keys in `appsettings.json`.

Initialize user secrets once:

```powershell
dotnet user-secrets init --project AselDevBlazorArchitecture.Web
```

Use a random JWT key of at least 32 characters. Production deployments should supply
the same settings through environment variables or a managed secret store.

## 4. PostgreSQL Setup

Create an empty PostgreSQL database and a dedicated application user. Grant that user
only the permissions required by the application and your migration process.

Example development connection string:

```text
Host=localhost;Port=5432;Database=aseldev_app;Username=aseldev_user;Password=CHANGE_ME;SSL Mode=Prefer
```

Configure local secrets:

```powershell
dotnet user-secrets set "Database:Enabled" "true" --project AselDevBlazorArchitecture.Web
dotnet user-secrets set "Database:Provider" "postgresql" --project AselDevBlazorArchitecture.Web
dotnet user-secrets set "Database:ConnectionString" "Host=localhost;Port=5432;Database=aseldev_app;Username=aseldev_user;Password=CHANGE_ME;SSL Mode=Prefer" --project AselDevBlazorArchitecture.Web
dotnet user-secrets set "JwtSettings:Key" "REPLACE_WITH_A_RANDOM_SECRET_OF_AT_LEAST_32_CHARACTERS" --project AselDevBlazorArchitecture.Web
```

Accepted PostgreSQL provider names:

```text
postgresql
postgres
npgsql
```

Inspect the PostgreSQL migration history without connecting:

```powershell
.\Scripts\Invoke-DatabaseMigrations.ps1 -Provider PostgreSql -Action List
```

Generate SQL for review:

```powershell
.\Scripts\Invoke-DatabaseMigrations.ps1 -Provider PostgreSql -Action Script
```

Apply migrations explicitly:

```powershell
.\Scripts\Invoke-DatabaseMigrations.ps1 `
  -Provider PostgreSql `
  -Action Update `
  -ConnectionString "Host=localhost;Port=5432;Database=aseldev_app;Username=aseldev_user;Password=CHANGE_ME;SSL Mode=Prefer"
```

## 5. MySQL Setup

Create an empty MySQL database using `utf8mb4` and a dedicated application user.

Example development connection string:

```text
server=localhost;port=3306;database=aseldev_app;uid=aseldev_user;pwd=CHANGE_ME;CharSet=utf8mb4;SslMode=Preferred;
```

Configure local secrets:

```powershell
dotnet user-secrets set "Database:Enabled" "true" --project AselDevBlazorArchitecture.Web
dotnet user-secrets set "Database:Provider" "mysql" --project AselDevBlazorArchitecture.Web
dotnet user-secrets set "Database:ConnectionString" "server=localhost;port=3306;database=aseldev_app;uid=aseldev_user;pwd=CHANGE_ME;CharSet=utf8mb4;SslMode=Preferred;" --project AselDevBlazorArchitecture.Web
dotnet user-secrets set "JwtSettings:Key" "REPLACE_WITH_A_RANDOM_SECRET_OF_AT_LEAST_32_CHARACTERS" --project AselDevBlazorArchitecture.Web
```

Accepted MySQL provider names:

```text
mysql
mariadb
```

Inspect or apply the independent MySQL history:

```powershell
.\Scripts\Invoke-DatabaseMigrations.ps1 -Provider MySql -Action List

.\Scripts\Invoke-DatabaseMigrations.ps1 `
  -Provider MySql `
  -Action Update `
  -ConnectionString "server=localhost;port=3306;database=aseldev_app;uid=aseldev_user;pwd=CHANGE_ME;CharSet=utf8mb4;SslMode=Preferred;"
```

The inherited MySQL `EmployeeId` migration should be reviewed before a production
deployment because its original `longtext` default can be ignored by some MySQL
versions. PostgreSQL does not use that migration history.

## 6. Add Future Migrations

Change entities and mappings in Infrastructure first. Then add the same logical schema
change independently to every provider you support.

PostgreSQL:

```powershell
.\Scripts\Invoke-DatabaseMigrations.ps1 `
  -Provider PostgreSql `
  -Action Add `
  -MigrationName AddFeatureName
```

MySQL:

```powershell
.\Scripts\Invoke-DatabaseMigrations.ps1 `
  -Provider MySql `
  -Action Add `
  -MigrationName AddFeatureName
```

After scaffolding:

1. Review both generated migrations.
2. Generate SQL scripts for both providers.
3. Confirm provider-specific types, indexes, defaults, and identity behavior.
4. Build the complete solution.
5. Apply migrations only to disposable or approved databases first.

Do not copy a generated migration from one provider project into the other.

## 7. Switching Providers

Changing `Database:Provider` changes the active EF provider and migration assembly. It
does not transfer existing data.

To move an application from MySQL to PostgreSQL:

1. Provision a new PostgreSQL database.
2. Apply the PostgreSQL migration history.
3. Export and transform MySQL data with a deliberate migration process.
4. Validate Identity users, normalized fields, roles, foreign keys, timestamps, and
   generated IDs.
5. Switch application configuration only after data validation.
6. Keep the old database available for rollback according to your deployment policy.

## 8. Administrator Provisioning

The template does not contain a default administrator password and does not seed or
reset an administrator during startup.

After applying migrations, provision the first administrator using an approved
bootstrap command or identity-management workflow. Do not add a fixed password to a
migration, source file, or committed configuration file.

## 9. Production Checklist

- [ ] Database credentials come from a secret store or environment variables.
- [ ] The database user follows least privilege.
- [ ] TLS/SSL is required according to the hosting environment.
- [ ] The correct provider-specific migration SQL was reviewed.
- [ ] A backup or rollback plan exists before migration.
- [ ] Migrations run as an explicit deployment step.
- [ ] The web process does not need schema-owner privileges after deployment.
- [ ] The JWT signing key is random, secret, and at least 32 characters.
- [ ] The first administrator is provisioned without committed credentials.
- [ ] Database connectivity and authentication are tested after deployment.

## 10. Troubleshooting

### Application starts but login returns unavailable

Confirm `Database:Enabled` is `true` in the effective environment configuration.

### Provider is not supported

Use `postgresql`, `postgres`, `npgsql`, `mysql`, or `mariadb`.

### No migrations are found

Use the matching provider in `Invoke-DatabaseMigrations.ps1`. Confirm the two migration
projects build and that the repository-local tool was restored.

### Build output is locked

Stop the running application or build to a separate verification output directory.
Migration operations should be performed while the target project is not being edited
or rebuilt by another process.

### Migration succeeds but application cannot connect

Check host, port, database name, username, password, firewall rules, TLS mode, and the
effective configuration source used by the running process.

