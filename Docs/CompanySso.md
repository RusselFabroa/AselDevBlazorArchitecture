# AselDev Blazor Company SSO

This project can act as the company Identity app for internal Blazor/API modules.

## Identity App Responsibilities

- Own user accounts, passwords, roles, and active/inactive status.
- Issue JWT access tokens after Employee ID / username login.
- Expose the current user profile through a protected SSO endpoint.
- Keep user registration/admin creation centralized.

## Template Modes

The template supports two SSO modes through `appsettings.json`.

Use this for the main Company DX Portal:

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

Use this for cloned module apps:

```json
"Sso": {
  "Mode": "Client",
  "Authority": "https://company-portal",
  "LoginUrl": "https://company-portal/login",
  "UserInfoUrl": "https://company-portal/api/sso/me",
  "AllowedReturnHosts": []
}
```

In `Client` mode, protected pages redirect to the portal login and local user administration is hidden.

## Cross-App Return URLs

For subdomain apps such as:

```text
https://tpc-dx.cloud
https://leave-app.tpc-dx.cloud
https://shuttle-app.tpc-dx.cloud
```

the IdentityProvider must allow trusted return hosts:

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

Client apps send their full current URL to the portal login:

```text
https://tpc-dx.cloud/login?urlReturn=https%3A%2F%2Fleave-app.tpc-dx.cloud%2Fprotected-page
```

The portal redirects only to hosts listed in `AllowedReturnHosts`. Do not allow every external URL, because that creates an open redirect risk.

## SSO Endpoints

Base route:

```text
/api/sso
```

Discovery:

```http
GET /api/sso/.well-known
```

Login/token:

```http
POST /api/sso/token
Content-Type: application/json

{
  "usernameOrEmployeeId": "admin",
  "password": "Admin@12345!"
}
```

Current user:

```http
GET /api/sso/me
Authorization: Bearer {access_token}
```

## Token Claims

Other company apps can trust these claims after validating the JWT signature, issuer, audience, and expiry:

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

## Other App Setup

Other ASP.NET Core apps should configure JWT bearer auth using the same issuer, audience, and signing key from this Identity app:

```csharp
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "AselDevBlazorArchitecture",
            ValidAudience = "AselDevBlazorArchitectureUsers",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("same-signing-key"))
        };
    });
```

Then protect controllers/pages with:

```csharp
[Authorize]
```

or:

```csharp
[Authorize(Roles = "Admin")]
```
