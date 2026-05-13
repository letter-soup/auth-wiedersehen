# Prompt 06 — API: IdentityResources CRUD

## Context

Continuing from Prompts 01–05. We now add full CRUD for **Duende IdentityServer Identity Resources**.

Identity Resources (`IdentityResources` table in `ConfigurationDB`) represent identity-related
scopes (e.g. `openid`, `profile`, `email`) that map to user claims returned in the ID token.
Each identity resource has user claims and custom properties as child collections.
Use the already-registered `ConfigurationDbContext`.

## Endpoints

All endpoints require `[Authorize]`.

| Method | Route                                             | Description                          |
|--------|---------------------------------------------------|--------------------------------------|
| GET    | /api/v1/identity-resources                        | Paginated list                       |
| POST   | /api/v1/identity-resources                        | Create identity resource             |
| GET    | /api/v1/identity-resources/{id}                   | Full details with child collections  |
| PUT    | /api/v1/identity-resources/{id}                   | Update scalar properties             |
| DELETE | /api/v1/identity-resources/{id}                   | Delete with cascaded children        |
| GET    | /api/v1/identity-resources/{id}/claims            | List user claim types                |
| POST   | /api/v1/identity-resources/{id}/claims            | Add user claim type                  |
| DELETE | /api/v1/identity-resources/{id}/claims/{itemId}   | Remove claim type                    |
| GET    | /api/v1/identity-resources/{id}/properties        | List custom properties               |
| POST   | /api/v1/identity-resources/{id}/properties        | Add property                         |
| DELETE | /api/v1/identity-resources/{id}/properties/{itemId} | Remove property                   |

## DTOs

### IdentityResourceListItem

```csharp
public sealed record IdentityResourceListItem(
    int      Id,
    string   Name,
    string?  DisplayName,
    bool     Enabled,
    bool     Required,
    bool     ShowInDiscoveryDocument,
    DateTime Created
);
```

### IdentityResourceDetails

```csharp
public sealed record IdentityResourceDetails(
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
    bool      NonEditable,
    List<IdentityResourceClaimItem>    Claims,
    List<IdentityResourcePropertyItem> Properties
);

public sealed record IdentityResourceClaimItem(int Id, string Type);
public sealed record IdentityResourcePropertyItem(int Id, string Key, string Value);
```

### CreateIdentityResourceRequest

```csharp
public sealed record CreateIdentityResourceRequest(
    string   Name,          // required, max 200, unique
    string?  DisplayName,
    string?  Description,
    bool     Enabled,
    bool     Required,
    bool     Emphasize,
    bool     ShowInDiscoveryDocument,
    List<string> UserClaims  // added to IdentityResourceClaims
);
```

Validator: `Name` required, max 200, unique in `IdentityResources`.

### UpdateIdentityResourceRequest

Same as `CreateIdentityResourceRequest` without `Name`.

### Child request types

```csharp
public sealed record AddIdentityResourceClaimRequest(string Type);            // unique per resource
public sealed record AddIdentityResourcePropertyRequest(string Key, string Value); // Key unique per resource
```

## Pagination & Search

Same pattern as other resources: `?page=1&pageSize=20&search=<substring>`.
Search matches on `Name` or `DisplayName`.

## Code Structure

```
src/Auth.Wiedersehen.ConfigurationManager.Api/IdentityResources/
├── IdentityResourceController.cs
├── IdentityResourceModels.cs
├── Queries/
│   ├── IGetIdentityResourcesQuery.cs
│   ├── GetIdentityResourcesQuery.cs
│   ├── IGetIdentityResourceByIdQuery.cs
│   └── GetIdentityResourceByIdQuery.cs
└── Commands/
    ├── ICreateIdentityResourceCommand.cs
    ├── CreateIdentityResourceCommand.cs
    ├── IUpdateIdentityResourceCommand.cs
    ├── UpdateIdentityResourceCommand.cs
    ├── IDeleteIdentityResourceCommand.cs
    ├── DeleteIdentityResourceCommand.cs
    ├── IAddIdentityResourceClaimCommand.cs / AddIdentityResourceClaimCommand.cs
    ├── IRemoveIdentityResourceClaimCommand.cs / RemoveIdentityResourceClaimCommand.cs
    ├── IAddIdentityResourcePropertyCommand.cs / AddIdentityResourcePropertyCommand.cs
    └── IRemoveIdentityResourcePropertyCommand.cs / RemoveIdentityResourcePropertyCommand.cs
```

Register all in `WebAppExtensions.ConfigureServices`.

## Error Handling

- Resource not found → `404`
- Duplicate `Name` → `409`
- Duplicate claim type / property key → `409`
- Child item not found on DELETE → `404`

## Acceptance Criteria

- Full CRUD for IdentityResources including claims and properties.
- Paginated list with optional search works.
- Well-known resources (`openid`, `profile`, `email`) can be managed the same way as custom ones.
