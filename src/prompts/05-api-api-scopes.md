# Prompt 05 — API: ApiScopes CRUD

## Context

Continuing from Prompts 01–04. We now add full CRUD for **Duende IdentityServer API Scopes**.

API Scopes (`ApiScopes` table in `ConfigurationDB`) are the individual permissions that clients
can request and that API resources reference. Each scope has user claims and custom properties
as child collections. Use the already-registered `ConfigurationDbContext`.

## Endpoints

All endpoints require `[Authorize]`.

| Method | Route                                  | Description                          |
|--------|----------------------------------------|--------------------------------------|
| GET    | /api/v1/api-scopes                     | Paginated list                       |
| POST   | /api/v1/api-scopes                     | Create API scope                     |
| GET    | /api/v1/api-scopes/{id}                | Full details with child collections  |
| PUT    | /api/v1/api-scopes/{id}                | Update scalar properties             |
| DELETE | /api/v1/api-scopes/{id}                | Delete with cascaded children        |
| GET    | /api/v1/api-scopes/{id}/claims         | List user claim types                |
| POST   | /api/v1/api-scopes/{id}/claims         | Add user claim type                  |
| DELETE | /api/v1/api-scopes/{id}/claims/{itemId}| Remove claim type                    |
| GET    | /api/v1/api-scopes/{id}/properties     | List custom properties               |
| POST   | /api/v1/api-scopes/{id}/properties     | Add property                         |
| DELETE | /api/v1/api-scopes/{id}/properties/{itemId} | Remove property               |

## DTOs

### ApiScopeListItem

```csharp
public sealed record ApiScopeListItem(
    int      Id,
    string   Name,
    string?  DisplayName,
    bool     Enabled,
    bool     Required,
    bool     ShowInDiscoveryDocument,
    DateTime Created
);
```

### ApiScopeDetails

```csharp
public sealed record ApiScopeDetails(
    int      Id,
    bool     Enabled,
    string   Name,
    string?  DisplayName,
    string?  Description,
    bool     Required,
    bool     Emphasize,
    bool     ShowInDiscoveryDocument,
    DateTime  Created,
    DateTime? Updated,
    DateTime? LastAccessed,
    bool      NonEditable,
    List<ApiScopeClaimItem>    Claims,
    List<ApiScopePropertyItem> Properties
);

public sealed record ApiScopeClaimItem(int Id, string Type);
public sealed record ApiScopePropertyItem(int Id, string Key, string Value);
```

### CreateApiScopeRequest

```csharp
public sealed record CreateApiScopeRequest(
    string   Name,          // required, max 200, must be unique
    string?  DisplayName,
    string?  Description,
    bool     Enabled,
    bool     Required,
    bool     Emphasize,
    bool     ShowInDiscoveryDocument,
    List<string> UserClaims  // claim type strings — added to ApiScopeClaims
);
```

Validator: `Name` required, max 200, unique in `ApiScopes`.

### UpdateApiScopeRequest

Same as `CreateApiScopeRequest` without `Name`.

### Child request types

```csharp
public sealed record AddApiScopeClaimRequest(string Type);          // unique per scope
public sealed record AddApiScopePropertyRequest(string Key, string Value); // Key unique per scope
```

## Pagination & Search

Same pattern as Clients and ApiResources: `?page=1&pageSize=20&search=<substring>`.
Search matches on `Name` or `DisplayName`.

## Code Structure

```
src/Auth.Wiedersehen.ConfigurationManager.Api/ApiScopes/
├── ApiScopeController.cs
├── ApiScopeModels.cs
├── Queries/
│   ├── IGetApiScopesQuery.cs
│   ├── GetApiScopesQuery.cs
│   ├── IGetApiScopeByIdQuery.cs
│   └── GetApiScopeByIdQuery.cs
└── Commands/
    ├── ICreateApiScopeCommand.cs
    ├── CreateApiScopeCommand.cs
    ├── IUpdateApiScopeCommand.cs
    ├── UpdateApiScopeCommand.cs
    ├── IDeleteApiScopeCommand.cs
    ├── DeleteApiScopeCommand.cs
    ├── IAddApiScopeClaimCommand.cs / AddApiScopeClaimCommand.cs
    ├── IRemoveApiScopeClaimCommand.cs / RemoveApiScopeClaimCommand.cs
    ├── IAddApiScopePropertyCommand.cs / AddApiScopePropertyCommand.cs
    └── IRemoveApiScopePropertyCommand.cs / RemoveApiScopePropertyCommand.cs
```

Register all in `WebAppExtensions.ConfigureServices`.

## Error Handling

- Scope not found → `404`
- Duplicate `Name` → `409`
- Duplicate claim type / property key → `409`
- Child item not found on DELETE → `404`

## Acceptance Criteria

- Full CRUD works for ApiScopes.
- Claims and properties child collections function correctly.
- Paginated list with optional search works.
