# AselDev Blazor Clean Architecture Template

AselDev Blazor is an internal starter template for building company portals,
admin tools, and module applications with a clean architecture structure.

The template is designed for two main uses:

- A main Company DX Portal that owns login, users, roles, and SSO token issuing.
- Separate cloned module apps that can redirect login to the main portal.

## Optional Database Capability

The application starts without a database by default. Database-independent pages
remain available while local Identity and persistence return a controlled unavailable
response.

See [Database Setup Guide](Docs/DatabaseSetup.md) for complete PostgreSQL, MySQL,
migration, secrets, and deployment instructions.

Enable PostgreSQL or MySQL through user secrets, environment variables, or a deployment secret
store. Never commit credentials:

```powershell
dotnet user-secrets init --project AselDevBlazorArchitecture.Web
dotnet user-secrets set "Database:Enabled" "true" --project AselDevBlazorArchitecture.Web
dotnet user-secrets set "Database:Provider" "postgresql" --project AselDevBlazorArchitecture.Web
dotnet user-secrets set "Database:ConnectionString" "Host=localhost;Port=5432;Database=YOUR_DATABASE;Username=YOUR_USER;Password=YOUR_PASSWORD" --project AselDevBlazorArchitecture.Web
dotnet user-secrets set "JwtSettings:Key" "YOUR_RANDOM_SECRET_WITH_AT_LEAST_32_CHARACTERS" --project AselDevBlazorArchitecture.Web
```

Migrations and administrator provisioning are explicit operational steps. The web
application never runs them automatically during startup.

## Current Stack

- .NET 8 / Blazor Server
- MudBlazor
- ASP.NET Core Identity
- JWT authentication
- Entity Framework Core
- Pomelo MySQL provider
- Serilog file logging
- Clean Architecture project separation

## Solution Structure

```text
AselDevBlazorArchitecture.Domain
  Core entities and domain models.

AselDevBlazorArchitecture.Application
  DTOs, interfaces, service contracts, response wrappers, and use-case models.

AselDevBlazorArchitecture.Infrastructure
  EF Core DbContext, Identity, authentication implementations, logging, and
  external implementation details.

AselDevBlazorArchitecture.Migrations.PostgreSql
  Independent PostgreSQL migration history and design-time context factory.

AselDevBlazorArchitecture.Migrations.MySql
  Independent MySQL migration history and design-time context factory.

AselDevBlazorArchitecture.Web
  Blazor UI, layouts, pages, API controllers, startup configuration, and SSO
  endpoints.

Docs
  Extra architecture and SSO notes.
```

## Main Features

- Login by Username / Employee ID and password.
- Admin-only user creation.
- Admin user list and role assignment on the Users page.
- Snackbar feedback for user creation status.
- Role-based authorization.
- Friendly unauthorized page for logged-in users without access.
- Login return URL support through `urlReturn`.
- Startup fallback error UI with database and general troubleshooting guidance.
- SSO foundation for IdentityProvider and Client modes.
- Simplified sidebar and app bar profile menu.
- Modern template homepage with SSO feature summary.

## First-Time Setup

### 1. Requirements

Install:

- .NET 8 SDK
- PostgreSQL or MySQL only when persistence is enabled
- Repository-local EF Core CLI tool restored with `dotnet tool restore`

Check your SDK:

```powershell
dotnet --version
```

Install or update EF tools:

```powershell
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef
```

If Visual Studio Package Manager Console shows an EF tools/runtime mismatch,
run the update command above, then restart Visual Studio or the terminal.

### 2. Optionally Enable the Database

The default configuration keeps persistence disabled, so no database is required
to start the application. When PostgreSQL- or MySQL-backed Identity is needed, supply these values
through user secrets or your deployment environment:

```json
"Database": {
  "Enabled": true,
  "Provider": "postgresql",
  "ConnectionString": "SUPPLY_THROUGH_A_SECRET_PROVIDER"
}
```

For production, do not keep real passwords in committed `appsettings.json`.
Use environment variables, user secrets, or your deployment secret store.

Supported provider values are `postgresql` (aliases: `postgres`, `npgsql`) and
`mysql` (alias: `mariadb`). The app validates the provider, connection string,
and JWT signing key only when `Database:Enabled` is `true`.

PostgreSQL example:

```text
Host=localhost;Port=5432;Database=YOUR_DATABASE;Username=YOUR_USER;Password=YOUR_PASSWORD
```

MySQL example:

```text
server=localhost;port=3306;database=YOUR_DATABASE;uid=YOUR_USER;pwd=YOUR_PASSWORD;SslMode=Preferred;
```

The existing migration set originated from MySQL. Before enabling PostgreSQL in
a real deployment, use the independent PostgreSQL migration assembly rather than
applying provider-specific MySQL migrations unchanged.

### Provider-Specific Migrations

Migration histories are isolated into two projects:

```text
AselDevBlazorArchitecture.Migrations.PostgreSql
AselDevBlazorArchitecture.Migrations.MySql
```

Runtime provider selection automatically chooses the matching migration assembly.
Use the repository-local EF tool through the migration helper:

```powershell
# Inspect migrations without connecting to a database
.\Scripts\Invoke-DatabaseMigrations.ps1 -Provider PostgreSql -Action List
.\Scripts\Invoke-DatabaseMigrations.ps1 -Provider MySql -Action List

# Add a migration to only one provider history
.\Scripts\Invoke-DatabaseMigrations.ps1 -Provider PostgreSql -Action Add -MigrationName AddExample

# Generate reviewable idempotent SQL without applying it
.\Scripts\Invoke-DatabaseMigrations.ps1 -Provider PostgreSql -Action Script

# Explicitly update a chosen database
.\Scripts\Invoke-DatabaseMigrations.ps1 -Provider PostgreSql -Action Update -ConnectionString "YOUR_SECRET_CONNECTION_STRING"
```

The helper never updates a database unless `-Action Update` is explicitly selected
and a connection string is supplied. The web application never applies migrations
during startup.

### 3. Apply Migrations

From the solution root:

```powershell
dotnet ef database update --project AselDevBlazorArchitecture.Infrastructure --startup-project AselDevBlazorArchitecture.Web
```

Useful EF commands:

```powershell
dotnet ef migrations list --project AselDevBlazorArchitecture.Infrastructure --startup-project AselDevBlazorArchitecture.Web
dotnet ef migrations add MigrationName --project AselDevBlazorArchitecture.Infrastructure --startup-project AselDevBlazorArchitecture.Web
dotnet ef database update --project AselDevBlazorArchitecture.Infrastructure --startup-project AselDevBlazorArchitecture.Web
```

If you see an error like `Unknown column 'a.EmployeeId' in 'field list'`, the
application code expects a column that your database does not have yet. Run the
database update command above.

If EF cannot update because DLL files are locked, stop the running app first,
then run the migration again.

### 4. Provision Identity Explicitly

The web host does not create an administrator or reset passwords. Provision the
first administrator through a dedicated deployment/bootstrap command or another
approved identity-management process after applying migrations.

### 5. Run the App

```powershell
dotnet run --project AselDevBlazorArchitecture.Web
```

Default local URLs are usually:

```text
https://localhost:7176
http://localhost:5000
```

## Authentication Flow

Login accepts:

```text
Username / Employee ID
Password
```

The login page supports:

```text
/login?urlReturn=/protected-page
```

Behavior:

- Browser sessions use an HttpOnly ASP.NET Core Identity cookie. Session credentials
  are never stored in `localStorage` and are not readable by JavaScript.
- JWT bearer tokens are reserved for API and cross-application SSO calls.
- If the user is already authenticated, the login page redirects to `urlReturn`.
- If the user is anonymous and opens a protected page, the app redirects to
  `/login?urlReturn=...`.
- If the user is logged in but lacks the required role, the app shows the
  unauthorized page instead of sending them back to login.

## User Administration

User management is admin-only.

Open:

```text
/users
```

The admin can create users with:

- Employee ID / username
- Full name
- Email
- Department
- Role
- Temporary password

There is no public self-registration flow by default. This is intentional for
company/internal systems.

## SSO Modes

The template supports two SSO modes.

### IdentityProvider Mode

Use this for the main Company DX Portal.

```json
"Sso": {
  "Mode": "IdentityProvider",
  "Authority": "https://company-portal",
  "LoginUrl": "/login",
  "UserInfoUrl": "/api/sso/me",
  "AllowedReturnHosts": [
    "company-portal",
    "leave-app.company-portal",
    "shuttle-app.company-portal"
  ]
}
```

Responsibilities:

- Own users and passwords.
- Own roles.
- Issue JWT tokens.
- Provide the portal login page.
- Provide SSO endpoints for other apps.
- Redirect users back only to trusted app domains.
- Show the Users admin page.

### Client Mode

Use this for cloned module apps.

```json
"Sso": {
  "Mode": "Client",
  "Authority": "https://company-portal",
  "LoginUrl": "https://company-portal/login",
  "UserInfoUrl": "https://company-portal/api/sso/me",
  "AllowedReturnHosts": []
}
```

Behavior:

- Protected pages redirect to the main portal login.
- Local user administration is hidden.
- The module app trusts the main portal JWT settings.

### Cross-App Return URLs

When the main portal and modules use subdomains:

```text
https://tpc-dx.cloud
https://leave-app.tpc-dx.cloud
https://shuttle-app.tpc-dx.cloud
```

configure the main portal allow-list:

```json
"Sso": {
  "Mode": "IdentityProvider",
  "Authority": "https://tpc-dx.cloud",
  "LoginUrl": "/login",
  "UserInfoUrl": "/api/sso/me",
  "AllowedReturnHosts": [
    "tpc-dx.cloud",
    "leave-app.tpc-dx.cloud",
    "shuttle-app.tpc-dx.cloud"
  ]
}
```

Then client apps can redirect to:

```text
https://tpc-dx.cloud/login?urlReturn=https%3A%2F%2Fleave-app.tpc-dx.cloud%2Fsome-page
```

The portal will return only to hosts listed in `AllowedReturnHosts`. Never allow all external URLs.

## SSO Endpoints

Base route:

```text
/api/sso
```

Discovery:

```http
GET /api/sso/.well-known
```

Token:

```http
POST /api/sso/token
Content-Type: application/json

{
  "usernameOrEmployeeId": "YOUR_EMPLOYEE_ID",
  "password": "your-password"
}
```

Current user:

```http
GET /api/sso/me
Authorization: Bearer {access_token}
```

JWT claims include:

```text
sub
nameidentifier
name
email
preferred_username
employee_id
department
role
jti
```

## JWT Settings

Configure:

```json
"JwtSettings": {
  "Key": "CHANGE_THIS_TO_A_LONG_SECRET_KEY",
  "Issuer": "AselDevBlazorArchitecture",
  "Audience": "AselDevBlazorArchitectureUsers",
  "ExpiryInMinutes": 480
}
```

For cloned client apps to trust the portal token, they must use compatible
issuer, audience, and signing key validation.

Before production:

- Replace the default signing key.
- Store the key securely.
- Use HTTPS.
- Review token lifetime.
- Consider refresh tokens if users need long sessions across many apps.

## Logging

Serilog is configured in:

```text
AselDevBlazorArchitecture.Web/appsettings.json
```

Default log path:

```text
AselDevBlazorArchitecture.Web/SystemLogs/aseldevlogs-.log
```

Logs roll daily and keep recent files based on `retainedFileCountLimit`.

## Startup Error UI

`Program.cs` includes fallback startup error handling.

Database-related startup errors show guidance for:

- Enabling persistence without supplying `Database:ConnectionString`.
- Running EF migrations.
- Checking migration list.
- Stopping the app if DLLs are locked.
- MySQL SSL connection issues.
- Running migrations and administrator provisioning explicitly.

General startup errors show guidance for:

- Running `dotnet build`.
- Checking appsettings files.
- Checking secrets and environment variables.
- Reviewing logs.
- Checking service registration changes.
- Checking port conflicts.

## Creating a New Project From This Template

Recommended GitHub flow:

1. Upload this repository to GitHub.
2. Mark it as a template repository.
3. For each new app/module, click `Use this template`.
4. Configure the new app's database, JWT, and SSO mode.
5. Rename namespaces/project branding only after the app builds.

If you used normal `git clone`, remove the original remote:

```powershell
git remote -v
git remote remove origin
git remote add origin https://github.com/your-company/your-new-app.git
git branch -M main
git push -u origin main
```

If you want a completely fresh Git history:

```powershell
Remove-Item -Recurse -Force .git
git init
git add .
git commit -m "Initial commit"
git remote add origin https://github.com/your-company/your-new-app.git
git push -u origin main
```

## Renaming the Template After Clone

Use the included rename script from the cloned project root:

```powershell
.\Scripts\Rename-Template.ps1 -NewName CompanyDxPortal
```

Preview the changes first:

```powershell
.\Scripts\Rename-Template.ps1 -NewName CompanyDxPortal -WhatIf
```

What it does:

- Replaces `AselDevBlazorArchitecture` with the new name in source/config/docs files.
- Renames solution, project files, folders, and any file names that include the old name.
- Skips `.git`, `.vs`, `bin`, and `obj` while editing references.
- Removes `bin` and `obj` folders so the next build starts clean.

Before running it, close Visual Studio, VS Code, running `dotnet` apps, and any
file preview windows for the cloned folder. If a file is locked, the script
pauses before folder renames; close the locking app and run the same command
again.

Then run:

```powershell
dotnet restore CompanyDxPortal.slnx
dotnet build CompanyDxPortal.slnx
```

If you want to keep `bin` and `obj` folders:

```powershell
.\Scripts\Rename-Template.ps1 -NewName CompanyDxPortal -SkipClean
```

## Recommended Company Architecture

Use this template as:

```text
Company DX Portal
  Mode: IdentityProvider
  Owns: login, users, roles, news, launcher buttons, SSO token issuing

Module App 1
  Mode: Client
  Owns: module-specific pages and business logic
  Login: redirects to Company DX Portal

Module App 2
  Mode: Client
  Owns: module-specific pages and business logic
  Login: redirects to Company DX Portal
```

This keeps identity centralized while allowing each module to evolve as a
separate application.

## Clean Architecture Rules

Use this direction for dependencies:

```text
Web -> Application -> Domain
Web -> Infrastructure -> Application -> Domain
```

Keep these boundaries:

- Domain should not depend on EF Core, Identity, MudBlazor, or web concerns.
- Application should define contracts, DTOs, and use-case shapes.
- Infrastructure should implement database, Identity, logging, and external integrations.
- Web should handle UI, routing, controllers, and composition.

## Common Troubleshooting

### Invalid username/employee ID or password

Check that persistence is enabled, the database points to the expected environment,
and an administrator has been provisioned through the deployment/bootstrap process.

### Unknown column `EmployeeId`

Run:

```powershell
dotnet ef database update --project AselDevBlazorArchitecture.Infrastructure --startup-project AselDevBlazorArchitecture.Web
```

### EF tools version warning

Run:

```powershell
dotnet tool update --global dotnet-ef
```

### MySQL SSL authentication error

For local/internal development, add this only if allowed by your environment:

```text
SslMode=None;
```

For production, use the SSL mode required by your database/security team.

### App starts but user data is missing

Check:

- The app is connected to the correct database.
- Migrations are applied.
- Startup tasks/seeder ran.
- The Users page is opened by an Admin user.

## Additional Docs

- `Docs/CompanySso.md`
- `Docs/LayerPurpose.md`

## Template Readiness

This template is ready to use as an internal starter for new projects.

Recommended next hardening before production:

- Move seeded admin credentials to secure configuration.
- Move admin user management logic from the Razor page into an Application service.
- Add refresh token support if sessions must survive across many apps for long periods.
- Add a sample client-module configuration file.
- Add automated tests around auth, roles, and SSO endpoints.
