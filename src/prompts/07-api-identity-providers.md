# Prompt 07 — API: IdentityProviders CRUD

## Context

Continuing from Prompts 01–06. We now add full CRUD for **Duende IdentityServer Identity Providers**.

Identity Providers (`IdentityProviders` table in `ConfigurationDB`) represent external
authentication providers (e.g. Google, GitHub, AAD) configured for dynamic authentication.
The `Properties` column stores a serialised JSON dictionary of provider-specific settings.
Use the already-registered `ConfigurationDbContext`.

## Endpoints

All endpoints require `[Authorize]`.

| Method | Route                           | Description                         |
|--------|---------------------------------|-------------------------------------|
| GET    | /api/v1/identity-providers      | Paginated list                      |
| POST   | /api/v1/identity-providers      | Create identity provider            |
| GET    | /api/v1/identity-providers/{id} | Full details                        |
| PUT    | /api/v1/identity-providers/{id} | Update properties                   |
| DELETE | /api/v1/identity-providers/{id} | Delete identity provider            |

No child collection sub-routes — the provider's configuration is a single JSON dictionary
exposed as `Dictionary<string, string>` in the DTOs and stored serialised in `Properties` column.

## DTOs

### IdentityProviderListItem

```csharp
public sealed record IdentityProviderListItem(
    int      Id,
    string   Scheme,
    string?  DisplayName,
    bool     Enabled,
    string   Type,
    DateTime Created
);
```

### IdentityProviderDetails

```csharp
public sealed record IdentityProviderDetails(
    int      Id,
    string   Scheme,
    string?  DisplayName,
    bool     Enabled,
    string   Type,
    Dictionary<string, string> Properties,  // parsed from JSON column
    DateTime  Created,
    DateTime? Updated,
    DateTime? LastAccessed,
    bool      NonEditable
);
```

### CreateIdentityProviderRequest

```csharp
public sealed record CreateIdentityProviderRequest(
    string   Scheme,       // required, max 200, unique
    string?  DisplayName,
    bool     Enabled,
    string   Type,         // required, max 20 (e.g. "oidc")
    Dictionary<string, string> Properties  // serialised to JSON for storage
);
```

Validator:
- `Scheme`: required, max 200, unique in `IdentityProviders`
- `Type`: required, max 20

### UpdateIdentityProviderRequest

```csharp
public sealed record UpdateIdentityProviderRequest(
    string?  DisplayName,
    bool     Enabled,
    Dictionary<string, string> Properties  // replaces existing JSON
);
```

## JSON Serialisation

The `Properties` column in the database holds a JSON string. When reading, deserialise with
`System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(entity.Properties ?? "{}")`.
When writing, serialise with `JsonSerializer.Serialize(request.Properties ?? new())`.

## Pagination & Search

Same pattern: `?page=1&pageSize=20&search=<substring>`.
Search matches on `Scheme` or `DisplayName`.

## Code Structure

```
src/Auth.Wiedersehen.ConfigurationManager.Api/IdentityProviders/
├── IdentityProviderController.cs
├── IdentityProviderModels.cs
├── Queries/
│   ├── IGetIdentityProvidersQuery.cs
│   ├── GetIdentityProvidersQuery.cs
│   ├── IGetIdentityProviderByIdQuery.cs
│   └── GetIdentityProviderByIdQuery.cs
└── Commands/
    ├── ICreateIdentityProviderCommand.cs
    ├── CreateIdentityProviderCommand.cs
    ├── IUpdateIdentityProviderCommand.cs
    ├── UpdateIdentityProviderCommand.cs
    ├── IDeleteIdentityProviderCommand.cs
    └── DeleteIdentityProviderCommand.cs
```

Register all in `WebAppExtensions.ConfigureServices`.

## Error Handling

- Provider not found → `404`
- Duplicate `Scheme` → `409`
- Invalid JSON properties → `400`

## Acceptance Criteria

- Full CRUD for IdentityProviders.
- `Properties` round-trips correctly between `Dictionary<string, string>` and JSON column.
- Paginated list with search works.
