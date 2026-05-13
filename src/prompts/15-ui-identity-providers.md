# Prompt 15 — UI: Identity Providers Management

## Context

Continuing from Prompts 08–14. We now implement the Identity Providers management UI.

The Configuration Manager API (Prompt 07) exposes:
- `GET    /api/v1/identity-providers?page=&pageSize=&search=`
- `POST   /api/v1/identity-providers`
- `GET    /api/v1/identity-providers/{id}`
- `PUT    /api/v1/identity-providers/{id}`
- `DELETE /api/v1/identity-providers/{id}`

Identity providers have no sub-collection routes. Their configuration is a flat
`Dictionary<string, string>` (the `Properties` field).

## API Client

### `src/api/identityProviders.ts`

```typescript
import { apiRequest } from './client'
import type { PagedResult } from './clients'

export interface IdentityProviderListItem {
  id: number; scheme: string; displayName: string | null
  enabled: boolean; type: string; created: string
}

export interface IdentityProviderDetails {
  id: number; scheme: string; displayName: string | null
  enabled: boolean; type: string
  properties: Record<string, string>
  created: string; updated: string | null; lastAccessed: string | null; nonEditable: boolean
}

export interface CreateIdentityProviderRequest {
  scheme: string; displayName?: string; enabled: boolean
  type: string; properties: Record<string, string>
}

export interface UpdateIdentityProviderRequest {
  displayName?: string; enabled: boolean; properties: Record<string, string>
}

export const identityProvidersApi = {
  list:   (params: string)                         => apiRequest<PagedResult<IdentityProviderListItem>>(`/api/v1/identity-providers${params}`),
  get:    (id: number)                             => apiRequest<IdentityProviderDetails>(`/api/v1/identity-providers/${id}`),
  create: (body: CreateIdentityProviderRequest)    => apiRequest<IdentityProviderDetails>('/api/v1/identity-providers', { method: 'POST', body: JSON.stringify(body) }),
  update: (id: number, body: UpdateIdentityProviderRequest) =>
    apiRequest<void>(`/api/v1/identity-providers/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  delete: (id: number)                             => apiRequest<void>(`/api/v1/identity-providers/${id}`, { method: 'DELETE' }),
}
```

## Views

### `src/views/identityProviders/IdentityProvidersView.vue`

List page:
1. Heading "Identity Providers" + "New Provider" button (opens `CreateIdentityProviderDialog`)
2. Search input (debounced)
3. `DataTable` columns: Enabled (badge), Scheme, Display Name, Type, Created, Actions ("View")
4. `PaginationBar`
5. Info callout: "Identity providers enable external authentication (e.g. Google, GitHub, AAD)
   using dynamic provider configuration."

### `src/views/identityProviders/IdentityProviderDetailView.vue`

Detail page:

#### Basic Info
Fields: Enabled (toggle), Scheme (readonly), Display Name, Type (readonly — cannot change after creation).
"Save" → `PUT /api/v1/identity-providers/{id}` (only DisplayName and Enabled).

#### Provider Properties
Dynamic key-value editor for the `properties` dictionary.

Render as a table with two columns (Key / Value) where each row is editable inline or via
an "Edit" icon. Below the table:
- "Add Property" button: reveals an inline row with Key + Value inputs + Confirm + Cancel.
- Each existing row: Edit button (makes the row editable in-place) + Delete button (removes the key).
- "Save Properties" button: sends the whole updated dictionary via `PUT`.

> Properties are provider-specific. Common examples for OIDC:
> - `Authority` → issuer URL
> - `ClientId` → OAuth client ID registered with the provider
> - `ClientSecret` → OAuth client secret (treat as sensitive, render as `type="password"`)
> - `ResponseType` → e.g. `code`
> - `Scope` → space-separated scopes

Show a helper note: "Property names and values are provider-specific. Refer to the external
provider's Duende IdentityServer documentation."

Mark keys that look like secrets (containing "secret", "key", "password", "token" case-insensitively)
with a masked `type="password"` input and a toggle-visibility button.

#### Danger Zone
Delete button → `ConfirmDialog` → DELETE → navigate to list.

### `src/components/identityProviders/CreateIdentityProviderDialog.vue`

Multi-step or single modal form:
- Scheme (required, unique)
- Display Name
- Type (required, select or text input — common values: `oidc`, `saml2p`)
- Enabled (checkbox, default true)
- Initial Properties (dynamic key-value editor — at least one row to start):
  - Add Row button
  - Each row: Key input + Value input + Remove button

On submit → `POST /api/v1/identity-providers` → navigate to detail page.

## UX Details

- The key-value editor should be intuitive: clicking "Add Property" appends an empty editable row;
  pressing Enter or clicking a checkmark saves that row to local state; the "Save Properties"
  button persists the entire map to the API.
- When viewing an existing provider, properties with sensitive-looking key names should default
  to masked display with a toggle button (eye icon from lucide-vue-next).
- The `scheme` field serves as the external provider's identifier in cookie/scheme-based auth;
  explain this with `<p class="text-sm text-muted-foreground">` helper text below the field.

## Acceptance Criteria

- List loads identity providers with pagination and search.
- Create dialog captures all required fields and creates a new provider.
- Detail page shows all properties and allows editing the dictionary.
- Save Properties persists the updated dictionary without full page reload.
- Sensitive property values are masked by default.
- Delete navigates back to list.
