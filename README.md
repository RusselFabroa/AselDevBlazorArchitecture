# AselDevBlazorArchitecture

An enterprise-oriented **.NET 8 Blazor clean architecture starter** for internal portals, administration systems, and independently deployable business modules.

The template is intentionally usable **without a database**. Persistence, ASP.NET Core Identity, MySQL, and PostgreSQL can be enabled later without changing the Domain or Application layers. Database migrations and administrator provisioning remain explicit operational actions; the web host does not mutate a database during startup.

## Current Status

| Capability | Status | Notes |
|---|---|---|
| Clean Architecture boundaries | Implemented | Domain, Application, Infrastructure, Web, and provider-specific migrations are separated. |
| Database-free startup | Implemented | Default configuration starts with `Database:Enabled=false`. |
| PostgreSQL | Implemented, optional | Runtime provider support and an independent migration history are included. |
| MySQL / MariaDB | Implemented, optional | Pomelo runtime support and an independent migration history are included. |
| Automatic migrations at startup | Intentionally disabled | Migrations are applied only through an explicit command. |
| Automatic database seeding | Intentionally disabled | No default administrator or password is created or reset at startup. |
| Browser authentication | Implemented when database is enabled | Uses an HttpOnly ASP.NET Core Identity application cookie. |
| JWT API authentication | Implemented when database is enabled | Intended for API and cross-application SSO calls. |
| Role-based user administration | Implemented when database is enabled | `/users` and `/register` require the `Admin` role. |
| SSO foundation | Implemented foundation | Supports `IdentityProvider` and `Client` configuration modes; production trust and key management still require deployment configuration. |
| Serilog file logging | Implemented | Daily rolling logs with configurable retention. |
| Swagger/OpenAPI | Implemented | Available in the development pipeline. |
| Automated tests | Not yet included | Authentication, authorization, provider selection, and SSO tests are recommended next. |
| Production identity bootstrap | Not yet included | Provision the first administrator through an approved deployment/bootstrap process. |

> [!IMPORTANT]
> “Implemented” describes code present in the template. Production readiness still requires environment-specific secrets, HTTPS, database provisioning, migrations, administrator bootstrap, security review, and automated verification.

## What the Template Provides

- A public architecture landing page explaining the template's capabilities.
- A database-optional application startup path with controlled unavailable services when persistence is disabled.
- ASP.NET Core Identity with username or employee-ID login when persistence is enabled.
- HttpOnly cookie sessions for the browser; credentials are not stored in `localStorage`.
- JWT bearer authentication for API and SSO scenarios.
- Admin-only user creation, user listing, and role assignment.
- Friendly login return URLs and a separate unauthorized experience.
- Configurable SSO roles for a central company portal or a client module.
- PostgreSQL and MySQL migration histories isolated from one another.
- A safe migration helper that does not update a database unless explicitly requested.
- Central NuGet package version management through `Directory.Packages.props`.
- MudBlazor UI, Swagger/OpenAPI, and Serilog request/file logging.
- A PowerShell helper for renaming the template after cloning.
- Startup error guidance for configuration, database, migration, assembly-lock, and port problems.

## Technology Stack

- .NET 8 and ASP.NET Core 8
- Blazor Web App with Interactive Server components
- MudBlazor 9
- ASP.NET Core Identity
- Entity Framework Core 8
- Npgsql for PostgreSQL
- Pomelo Entity Framework Core provider for MySQL/MariaDB
- Cookie and JWT bearer authentication
- Serilog
- Swashbuckle / OpenAPI
- Central Package Management

Package versions are normalized in [`Directory.Packages.props`](Directory.Packages.props).

## Architecture

```text
Web ───────────────> Application ───────────────> Domain
 │                        ▲
 └──> Infrastructure ─────┘
          │
          ├──> Migrations.PostgreSql
          └──> Migrations.MySql
```

Dependencies point inward:

- **Domain** contains enterprise rules, entities, value concepts, and domain exceptions. It must not depend on EF Core, Identity, MudBlazor, or ASP.NET Core.
- **Application** contains contracts, DTOs, response models, and use-case boundaries. It depends only on Domain plus minimal abstractions.
- **Infrastructure** implements persistence, Identity, authentication services, provider selection, logging, and external concerns.
- **Web** owns UI composition, routing, controllers, authentication endpoints, and dependency registration.
- **Migration projects** own database-provider-specific EF Core history and design-time factories.

See [`Docs/LayerPurpose.md`](Docs/LayerPurpose.md) for the detailed boundary rules.

## Repository Structure

```text
AselDevBlazorArchitecture/
├── AselDevBlazorArchitecture.Domain/
├── AselDevBlazorArchitecture.Application/
├── AselDevBlazorArchitecture.Infrastructure/
├── AselDevBlazorArchitecture.Migrations.PostgreSql/
├── AselDevBlazorArchitecture.Migrations.MySql/
├── AselDevBlazorArchitecture.Web/
├── AselDevBlazorArchitecture.Client/
├── Docs/
├── Scripts/
├── .config/dotnet-tools.json
├── Directory.Packages.props
└── AselDevBlazorArchitecture.slnx
```

`AselDevBlazorArchitecture.Client` is a hosted WebAssembly sample retained in the repository but is not currently included in the main `.slnx`. The actively composed template host is `AselDevBlazorArchitecture.Web`.

## Quick Start Without a Database

### Requirements

- .NET 8 SDK
- PowerShell 7 for the included helper scripts
- PostgreSQL or MySQL only when enabling persistence

From the repository root:

```powershell
dotnet restore AselDevBlazorArchitecture.slnx
dotnet build AselDevBlazorArchitecture.slnx
dotnet run --project AselDevBlazorArchitecture.Web
```

The committed default is:

```json
"Database": {
  "Enabled": false,
  "Provider": "postgresql",
  "ConnectionString": ""
}
```

With persistence disabled:

- The public architecture page can run without a database server.
- EF Core and local Identity persistence are not registered.
- Login and user-administration services return controlled unavailable responses.
- No migration or seeding task runs in the background.

## Enabling Persistence

Never commit real credentials. Use .NET user secrets locally and a managed secret store or environment variables in deployment.

PostgreSQL example:

```powershell
dotnet user-secrets init --project AselDevBlazorArchitecture.Web
dotnet user-secrets set "Database:Enabled" "true" --project AselDevBlazorArchitecture.Web
dotnet user-secrets set "Database:Provider" "postgresql" --project AselDevBlazorArchitecture.Web
dotnet user-secrets set "Database:ConnectionString" "Host=localhost;Port=5432;Database=YOUR_DATABASE;Username=YOUR_USER;Password=YOUR_PASSWORD" --project AselDevBlazorArchitecture.Web
dotnet user-secrets set "JwtSettings:Key" "YOUR_RANDOM_SECRET_WITH_AT_LEAST_32_CHARACTERS" --project AselDevBlazorArchitecture.Web
```

MySQL example values:

```text
Database:Provider=mysql
Database:ConnectionString=server=localhost;port=3306;database=YOUR_DATABASE;uid=YOUR_USER;pwd=YOUR_PASSWORD;SslMode=Preferred;
```

Supported provider values:

| Provider | Accepted values | Migration project |
|---|---|---|
| PostgreSQL | `postgresql`, `postgres`, `npgsql` | `AselDevBlazorArchitecture.Migrations.PostgreSql` |
| MySQL / MariaDB | `mysql`, `mariadb` | `AselDevBlazorArchitecture.Migrations.MySql` |

When `Database:Enabled=true`, the application validates the selected provider, connection string, and JWT signing key during composition.

## Provider-Specific Migrations

PostgreSQL and MySQL migrations are intentionally independent. Do not apply one provider's migration history to the other provider.

Restore the repository-local EF tool and use the helper:

```powershell
dotnet tool restore

# Inspect without connecting to a database
.\Scripts\Invoke-DatabaseMigrations.ps1 -Provider PostgreSql -Action List
.\Scripts\Invoke-DatabaseMigrations.ps1 -Provider MySql -Action List

# Add a migration to one provider only
.\Scripts\Invoke-DatabaseMigrations.ps1 -Provider PostgreSql -Action Add -MigrationName AddExample

# Generate reviewable idempotent SQL
.\Scripts\Invoke-DatabaseMigrations.ps1 -Provider PostgreSql -Action Script

# Apply only when explicitly intended
.\Scripts\Invoke-DatabaseMigrations.ps1 -Provider PostgreSql -Action Update -ConnectionString "YOUR_SECRET_CONNECTION_STRING"
```

The `Update` action requires an explicit connection string. The web application never applies migrations automatically.

For full setup, switching, deployment, and troubleshooting instructions, see [`Docs/DatabaseSetup.md`](Docs/DatabaseSetup.md).

## Identity and Browser Sessions

When persistence is enabled, login accepts:

```text
Username / Employee ID
Password
```

Relevant routes:

| Route | Purpose | Access |
|---|---|---|
| `/` | Architecture landing page | Public |
| `/login` | Browser sign-in | Public |
| `/company-dx-portal` | Example company portal page | Application authorization rules |
| `/users` | User list and role administration | `Admin` |
| `/register` | Create a user | `Admin` |
| `/unauthorized` | Friendly access-denied page | Publicly routable |

Browser behavior:

- The browser receives an HttpOnly ASP.NET Core Identity cookie.
- JWT tokens are reserved for APIs and cross-application calls.
- Protected navigation retains `urlReturn`, such as `/login?urlReturn=/users`.
- Authenticated users without a required role see the unauthorized page rather than being sent through another login loop.
- With the database disabled, authentication-dependent actions report that the capability is unavailable.

No public self-registration is enabled by default.

## SSO Foundation

Two configuration modes are supported:

### IdentityProvider

Use for the central Company DX Portal that owns users, passwords, roles, login, and token issuing.

### Client

Use for independently deployed business modules that redirect authentication to the central portal and trust its token configuration.

SSO API base route:

```text
/api/sso
```

Available endpoints:

```http
GET  /api/sso/.well-known
POST /api/sso/token
GET  /api/sso/me
```

Cross-application return URLs are restricted to `Sso:AllowedReturnHosts`. Never configure an unrestricted external return URL.

See [`Docs/CompanySso.md`](Docs/CompanySso.md) for the flow, claims, endpoints, and client configuration.

## Logging and API Documentation

Serilog configuration lives in `AselDevBlazorArchitecture.Web/appsettings.json`.

Default rolling log location:

```text
AselDevBlazorArchitecture.Web/SystemLogs/aseldevlogs-.log
```

Swagger/OpenAPI services and middleware are configured for development. Do not expose development API documentation publicly without an explicit security decision.

## Creating a New Application

The recommended workflow is to mark this GitHub repository as a template, then use **Use this template** for each portal or module. This creates a separate repository without carrying the template's Git history or remote.

After creating the new repository:

1. Clone it.
2. Preview the rename operation.
3. Rename the template.
4. Restore and build.
5. Configure its database and SSO mode only if required.

Because the rename script currently has a legacy default prefix, pass the complete current name explicitly:

```powershell
.\Scripts\Rename-Template.ps1 `
  -OldName AselDevBlazorArchitecture `
  -NewName CompanyDxPortal `
  -WhatIf

.\Scripts\Rename-Template.ps1 `
  -OldName AselDevBlazorArchitecture `
  -NewName CompanyDxPortal

dotnet restore CompanyDxPortal.slnx
dotnet build CompanyDxPortal.slnx
```

Close Visual Studio, running `dotnet` processes, and file-preview windows before renaming. The script skips `.git`, `.vs`, `bin`, and `obj` while replacing text and removes build outputs unless `-SkipClean` is supplied.

## Recommended Enterprise Deployment Model

```text
Company DX Portal
  SSO mode: IdentityProvider
  Owns: users, roles, login, application launcher, JWT issuing

Business Module A
  SSO mode: Client
  Owns: its business pages and use cases
  Authentication: redirects to the Company DX Portal

Business Module B
  SSO mode: Client
  Owns: its business pages and use cases
  Authentication: redirects to the Company DX Portal
```

This keeps identity centralized while allowing business modules to be deployed and evolved independently.

## Production Checklist

Before using the template in production:

- [ ] Store database and JWT secrets outside committed configuration.
- [ ] Use HTTPS and secure cookie settings appropriate to the deployment topology.
- [ ] Review `Sso:AllowedReturnHosts`, issuer, audience, signing key, and token lifetime.
- [ ] Provision the target database explicitly.
- [ ] Review generated migration SQL before applying it.
- [ ] Provision the first administrator through an approved process.
- [ ] Confirm that Swagger is not unintentionally exposed.
- [ ] Add authentication, authorization, SSO, and provider-selection tests.
- [ ] Add health checks, monitoring, backup, and recovery procedures.
- [ ] Perform dependency, secret, and application security scanning.

## Current Limitations and Recommended Next Work

1. Add automated unit, integration, authorization, and provider-selection tests.
2. Add an explicit, auditable administrator bootstrap tool that is separate from web startup.
3. Complete production-grade SSO trust management, key rotation, and refresh/session strategy.
4. Decide whether to integrate the hosted WebAssembly sample into the solution or remove it from the template.
5. Update the rename script's default old name to `AselDevBlazorArchitecture` after its behavior is tested end to end.
6. Add deployment-specific health checks and observability exports.

## Documentation

- [`Docs/DatabaseSetup.md`](Docs/DatabaseSetup.md) — PostgreSQL, MySQL, migrations, secrets, and deployment
- [`Docs/CompanySso.md`](Docs/CompanySso.md) — identity-provider/client modes and SSO endpoints
- [`Docs/LayerPurpose.md`](Docs/LayerPurpose.md) — layer responsibilities and dependency rules
- [`AselDevBlazorArchitecture.Web/Docs/LayerPurpose.md`](AselDevBlazorArchitecture.Web/Docs/LayerPurpose.md) — in-application layer summary

## License

See [`LICENSE.txt`](LICENSE.txt).
