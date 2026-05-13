# Prompt 03 — API: Clients CRUD

## Context

Continuing from Prompts 01–02. The project `Auth.Wiedersehen.ConfigurationManager.Api` has JWT
auth in place. We now add full CRUD for **Duende IdentityServer Clients**.

Clients are stored in the existing Duende `ConfigurationDB` database using
`Duende.IdentityServer.EntityFramework.DbContexts.ConfigurationDbContext`. This DbContext is not
owned by this API — it already exists and is managed by the main `Auth.Wiedersehen` project. This
API only reads and writes to it; it **must not** create or modify EF migrations for it.

Register `ConfigurationDbContext` in `WebAppExtensions.ConfigureServices`:
```csharp
builder.Services.AddDbContext<ConfigurationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(ConfigurationKey.ConnectionString.ConfigurationDb)
    )
);
```
Do **not** call `MigrationsAssembly` here — Duende owns these migrations.

## Endpoints

All endpoints require `[Authorize]`.

### Clients collection

| Method | Route                  | Description                        |
|--------|------------------------|------------------------------------|
| GET    | /api/v1/clients        | Paginated list of clients          |
| POST   | /api/v1/clients        | Create a new client                |

### Single client

| Method | Route                  | Description                        |
|--------|------------------------|------------------------------------|
| GET    | /api/v1/clients/{id}   | Get client with all child collections |
| PUT    | /api/v1/clients/{id}   | Update client core properties      |
| DELETE | /api/v1/clients/{id}   | Delete client and all children     |

### Child collections (pattern: replace `{col}` with the name below)

Route pattern: `/api/v1/clients/{id}/{col}`

| Col                        | Child Table                  |
|----------------------------|------------------------------|
| `secrets`                  | ClientSecrets                |
| `scopes`                   | ClientScopes                 |
| `redirect-uris`            | ClientRedirectUris           |
| `post-logout-redirect-uris`| ClientPostLogoutRedirectUris |
| `cors-origins`             | ClientCorsOrigins            |
| `grant-types`              | ClientGrantTypes             |
| `claims`                   | ClientClaims                 |
| `properties`               | ClientProperties             |
| `idp-restrictions`         | ClientIdPRestrictions        |

Each child collection has:
- `GET  /api/v1/clients/{id}/{col}` — list all items
- `POST /api/v1/clients/{id}/{col}` — add item
- `DELETE /api/v1/clients/{id}/{col}/{itemId}` — remove item by child PK

## DTOs

### ClientListItem (used in paginated GET /api/v1/clients)

```csharp
public sealed record ClientListItem(
    int    Id,
    string ClientId,
    string? ClientName,
    bool   Enabled,
    string ProtocolType,
    DateTime Created
);
```

### ClientDetails (used in GET /api/v1/clients/{id})

All scalar properties from the Clients table plus the child collections as embedded lists:

```csharp
public sealed record ClientDetails(
    int      Id,
    bool     Enabled,
    string   ClientId,
    string   ProtocolType,
    bool     RequireClientSecret,
    string?  ClientName,
    string?  Description,
    string?  ClientUri,
    string?  LogoUri,
    bool     RequireConsent,
    bool     AllowRememberConsent,
    bool     AlwaysIncludeUserClaimsInIdToken,
    bool     RequirePkce,
    bool     AllowPlainTextPkce,
    bool     RequireRequestObject,
    bool     AllowAccessTokensViaBrowser,
    bool     RequireDPoP,
    string?  FrontChannelLogoutUri,
    bool     FrontChannelLogoutSessionRequired,
    string?  BackChannelLogoutUri,
    bool     BackChannelLogoutSessionRequired,
    bool     AllowOfflineAccess,
    int      IdentityTokenLifetime,
    string?  AllowedIdentityTokenSigningAlgorithms,
    int      AccessTokenLifetime,
    int      AuthorizationCodeLifetime,
    int?     ConsentLifetime,
    int      AbsoluteRefreshTokenLifetime,
    int      SlidingRefreshTokenLifetime,
    int      RefreshTokenUsage,
    bool     UpdateAccessTokenClaimsOnRefresh,
    int      RefreshTokenExpiration,
    int      AccessTokenType,
    bool     EnableLocalLogin,
    bool     IncludeJwtId,
    bool     AlwaysSendClientClaims,
    string?  ClientClaimsPrefix,
    string?  PairWiseSubjectSalt,
    string?  InitiateLoginUri,
    int?     UserSsoLifetime,
    string?  UserCodeType,
    int      DeviceCodeLifetime,
    int?     CibaLifetime,
    int?     PollingInterval,
    bool?    CoordinateLifetimeWithUserSession,
    bool     RequirePushedAuthorization,
    int?     PushedAuthorizationLifetime,
    DateTime Created,
    DateTime? Updated,
    DateTime? LastAccessed,
    bool     NonEditable,
    // Child collections
    List<ClientSecretItem>             Secrets,
    List<string>                       Scopes,
    List<string>                       RedirectUris,
    List<string>                       PostLogoutRedirectUris,
    List<string>                       CorsOrigins,
    List<string>                       GrantTypes,
    List<ClientClaimItem>              Claims,
    List<ClientPropertyItem>           Properties,
    List<string>                       IdpRestrictions
);

public sealed record ClientSecretItem(int Id, string? Description, string Type, string Value, DateTime? Expiration, DateTime Created);
public sealed record ClientClaimItem(int Id, string Type, string Value);
public sealed record ClientPropertyItem(int Id, string Key, string Value);
```

> **Note on secrets:** never return the raw `Value` hash for existing secrets. Return a masked
> placeholder like `"***"` for display. Only include the actual value in the POST response (once).

### CreateClientRequest

```csharp
public sealed record CreateClientRequest(
    string  ClientId,                   // required, max 200
    string  ProtocolType,               // required, default "oidc"
    bool    RequireClientSecret,        // default true
    string? ClientName,
    string? Description,
    string? ClientUri,
    string? LogoUri,
    bool    RequireConsent,
    bool    AllowRememberConsent,
    bool    RequirePkce,
    bool    AllowOfflineAccess,
    bool    EnableLocalLogin,
    List<string> AllowedGrantTypes,     // required, at least one
    List<string> RedirectUris,
    List<string> PostLogoutRedirectUris,
    List<string> AllowedScopes,
    List<string> AllowedCorsOrigins
);
```

Defaults for omitted lifetime fields: use Duende's defaults (IdentityTokenLifetime=300,
AccessTokenLifetime=3600, AuthorizationCodeLifetime=300, etc.).

Validator (FluentValidation):
- `ClientId`: required, max 200, must not already exist in `ConfigurationDbContext.Clients`
- `ProtocolType`: required, max 200
- `AllowedGrantTypes`: must have at least one item

### UpdateClientRequest

Same fields as `CreateClientRequest` excluding `ClientId` (which cannot be changed).

### Child collection request types

```csharp
// POST /api/v1/clients/{id}/secrets
public sealed record AddClientSecretRequest(
    string? Description,
    string  Value,        // plain text — hash with SHA-256 before storing
    string  Type,         // default "SharedSecret"
    DateTime? Expiration
);
public sealed record AddedClientSecretResponse(int Id, DateTime Created);

// POST /api/v1/clients/{id}/scopes
public sealed record AddClientScopeRequest(string Scope);

// POST /api/v1/clients/{id}/redirect-uris
public sealed record AddRedirectUriRequest(string RedirectUri);

// POST /api/v1/clients/{id}/post-logout-redirect-uris
public sealed record AddPostLogoutRedirectUriRequest(string PostLogoutRedirectUri);

// POST /api/v1/clients/{id}/cors-origins
public sealed record AddCorsOriginRequest(string Origin);

// POST /api/v1/clients/{id}/grant-types
public sealed record AddGrantTypeRequest(string GrantType);

// POST /api/v1/clients/{id}/claims
public sealed record AddClientClaimRequest(string Type, string Value);

// POST /api/v1/clients/{id}/properties
public sealed record AddClientPropertyRequest(string Key, string Value);

// POST /api/v1/clients/{id}/idp-restrictions
public sealed record AddIdpRestrictionRequest(string Provider);
```

## Pagination

`GET /api/v1/clients` accepts `?page=1&pageSize=20` query params.
Response:
```csharp
public sealed record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);
```

Optional filter: `?search=<substring>` — case-insensitive match on `ClientId` or `ClientName`.

## Secret Hashing

When adding a client secret via POST, hash the plain-text value before persisting:
```csharp
using var sha = SHA256.Create();
var hash = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(value)));
// prefix per Duende convention: "SharedSecret".Sha256() equivalent
```
Use `Duende.IdentityServer.Models.Secret` helper methods if available in the package, otherwise
compute manually. Store the hash in `ClientSecrets.Value`.

## Code Structure

```
src/Auth.Wiedersehen.ConfigurationManager.Api/Clients/
├── ClientController.cs          # all endpoints
├── ClientModels.cs              # all DTOs and validators
├── Queries/
│   ├── IGetClientsQuery.cs
│   ├── GetClientsQuery.cs
│   ├── IGetClientByIdQuery.cs
│   └── GetClientByIdQuery.cs
└── Commands/
    ├── ICreateClientCommand.cs
    ├── CreateClientCommand.cs
    ├── IUpdateClientCommand.cs
    ├── UpdateClientCommand.cs
    ├── IDeleteClientCommand.cs
    ├── DeleteClientCommand.cs
    └── (one IAdd/Add + IRemove/Remove command per child collection)
```

Register all commands and queries in `WebAppExtensions.ConfigureServices`.

## Error Handling

- Client not found → `404 Not Found` with `ErrorDetails`
- Duplicate `ClientId` on create → `409 Conflict`
- Duplicate child item (e.g. duplicate scope) → `409 Conflict`
- Validation errors → `400 Bad Request` via `HttpResponseExceptionFilter`
- Child item not found on DELETE → `404 Not Found`

## Acceptance Criteria

- `GET /api/v1/clients` returns paginated list, requires JWT.
- `POST /api/v1/clients` creates a client with all child items provided.
- `GET /api/v1/clients/{id}` returns full details with all child collections.
- `PUT /api/v1/clients/{id}` updates scalar properties.
- `DELETE /api/v1/clients/{id}` cascades to all child tables.
- All child collection endpoints function correctly.
- Secrets are stored hashed and never returned in plain text after creation.
