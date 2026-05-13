# Prompt 04 — API: ApiResources CRUD

## Context

Continuing from Prompts 01–03. JWT auth and the Clients feature are in place.
We now add full CRUD for **Duende IdentityServer API Resources**.

API Resources (`ApiResources` table in `ConfigurationDB`) represent protected APIs that
clients can request access to. Each resource has child collections: scopes, user claims,
secrets, and custom properties. Use `ConfigurationDbContext` (already registered) — no new
migrations needed.

## Endpoints

All endpoints require `[Authorize]`.

| Method | Route                                   | Description                          |
|--------|-----------------------------------------|--------------------------------------|
| GET    | /api/v1/api-resources                   | Paginated list                       |
| POST   | /api/v1/api-resources                   | Create API resource                  |
| GET    | /api/v1/api-resources/{id}              | Full details with child collections  |
| PUT    | /api/v1/api-resources/{id}              | Update scalar properties             |
| DELETE | /api/v1/api-resources/{id}              | Delete with cascaded children        |
| GET    | /api/v1/api-resources/{id}/scopes       | List associated scopes               |
| POST   | /api/v1/api-resources/{id}/scopes       | Add scope to resource                |
| DELETE | /api/v1/api-resources/{id}/scopes/{itemId} | Remove scope                      |
| GET    | /api/v1/api-resources/{id}/claims       | List user claim types                |
| POST   | /api/v1/api-resources/{id}/claims       | Add user claim type                  |
| DELETE | /api/v1/api-resources/{id}/claims/{itemId} | Remove claim type                 |
| GET    | /api/v1/api-resources/{id}/secrets      | List secrets (values masked)         |
| POST   | /api/v1/api-resources/{id}/secrets      | Add secret (hash before storing)     |
| DELETE | /api/v1/api-resources/{id}/secrets/{itemId} | Remove secret                   |
| GET    | /api/v1/api-resources/{id}/properties   | List custom properties               |
| POST   | /api/v1/api-resources/{id}/properties   | Add property                         |
| DELETE | /api/v1/api-resources/{id}/properties/{itemId} | Remove property             |

## DTOs

### ApiResourceListItem

```csharp
public sealed record ApiResourceListItem(
    int      Id,
    string   Name,
    string?  DisplayName,
    bool     Enabled,
    bool     ShowInDiscoveryDocument,
    DateTime Created
);
```

### ApiResourceDetails

```csharp
public sealed record ApiResourceDetails(
    int      Id,
    bool     Enabled,
    string   Name,
    string?  DisplayName,
    string?  Description,
    string?  AllowedAccessTokenSigningAlgorithms,
    bool     ShowInDiscoveryDocument,
    bool     RequireResourceIndicator,
    DateTime  Created,
    DateTime? Updated,
    DateTime? LastAccessed,
    bool      NonEditable,
    List<ApiResourceScopeItem>    Scopes,
    List<string>                  UserClaims,
    List<ApiResourceSecretItem>   Secrets,
    List<ApiResourcePropertyItem> Properties
);

public sealed record ApiResourceScopeItem(int Id, string Scope);
public sealed record ApiResourceSecretItem(int Id, string Type, string? Description, DateTime? Expiration, DateTime Created);
public sealed record ApiResourcePropertyItem(int Id, string Key, string Value);
```

### CreateApiResourceRequest

```csharp
public sealed record CreateApiResourceRequest(
    string   Name,           // required, max 200, must be unique
    string?  DisplayName,
    string?  Description,
    bool     Enabled,
    bool     ShowInDiscoveryDocument,
    bool     RequireResourceIndicator,
    string?  AllowedAccessTokenSigningAlgorithms,
    List<string> Scopes,     // scope names
    List<string> UserClaims  // claim type strings
);
```

Validator: `Name` required, max 200, unique in `ApiResources`.

### UpdateApiResourceRequest

Same fields as `CreateApiResourceRequest` excluding `Name`.

### Child request types

```csharp
public sealed record AddApiResourceScopeRequest(string Scope);                  // unique per resource
public sealed record AddApiResourceClaimRequest(string Type);                   // unique per resource
public sealed record AddApiResourceSecretRequest(
    string   Value,        // plain text — hash SHA-256 before storing
    string   Type,         // default "SharedSecret"
    string?  Description,
    DateTime? Expiration
);
public sealed record AddApiResourceSecretResponse(int Id, DateTime Created);
public sealed record AddApiResourcePropertyRequest(string Key, string Value);  // Key unique per resource
```

## Pagination & Search

Same pattern as Clients: `?page=1&pageSize=20&search=<substring>`.
Search matches on `Name` or `DisplayName`.

## Secret Hashing

Apply the same SHA-256 hashing strategy as in Prompt 03 for `ApiResourceSecrets.Value`.

## Code Structure

```
src/Auth.Wiedersehen.ConfigurationManager.Api/ApiResources/
├── ApiResourceController.cs
├── ApiResourceModels.cs
├── Queries/
│   ├── IGetApiResourcesQuery.cs
│   ├── GetApiResourcesQuery.cs
│   ├── IGetApiResourceByIdQuery.cs
│   └── GetApiResourceByIdQuery.cs
└── Commands/
    ├── ICreateApiResourceCommand.cs
    ├── CreateApiResourceCommand.cs
    ├── IUpdateApiResourceCommand.cs
    ├── UpdateApiResourceCommand.cs
    ├── IDeleteApiResourceCommand.cs
    ├── DeleteApiResourceCommand.cs
    └── (Add/Remove commands for each child collection)
```

Register all in `WebAppExtensions.ConfigureServices`.

## Error Handling

- Resource not found → `404`
- Duplicate `Name` → `409`
- Duplicate scope/claim/property key → `409`
- Child item not found → `404`

## Acceptance Criteria

- Full CRUD works for ApiResources.
- All four child collection endpoints function correctly.
- Secrets stored as SHA-256 hashes, never returned in plain text.
- Paginated list with optional search works.
