# Prompt 12 — UI: API Resources Management

## Context

Continuing from Prompts 08–11. App shell and Clients UI are in place.
We now implement the API Resources management UI.

The Configuration Manager API (Prompt 04) exposes:
- `GET    /api/v1/api-resources?page=&pageSize=&search=`
- `POST   /api/v1/api-resources`
- `GET    /api/v1/api-resources/{id}`
- `PUT    /api/v1/api-resources/{id}`
- `DELETE /api/v1/api-resources/{id}`
- Child collection endpoints: `/scopes`, `/claims`, `/secrets`, `/properties`

## API Client

### `src/api/apiResources.ts`

```typescript
import { apiRequest } from './client'
import type { PagedResult } from './clients'

export interface ApiResourceListItem {
  id: number; name: string; displayName: string | null
  enabled: boolean; showInDiscoveryDocument: boolean; created: string
}

export interface ApiResourceScopeItem    { id: number; scope: string }
export interface ApiResourceSecretItem   { id: number; type: string; description: string | null; expiration: string | null; created: string }
export interface ApiResourcePropertyItem { id: number; key: string; value: string }

export interface ApiResourceDetails {
  id: number; enabled: boolean; name: string; displayName: string | null
  description: string | null; allowedAccessTokenSigningAlgorithms: string | null
  showInDiscoveryDocument: boolean; requireResourceIndicator: boolean
  created: string; updated: string | null; lastAccessed: string | null; nonEditable: boolean
  scopes: ApiResourceScopeItem[]; userClaims: string[]
  secrets: ApiResourceSecretItem[]; properties: ApiResourcePropertyItem[]
}

export interface CreateApiResourceRequest {
  name: string; displayName?: string; description?: string
  enabled: boolean; showInDiscoveryDocument: boolean; requireResourceIndicator: boolean
  allowedAccessTokenSigningAlgorithms?: string
  scopes: string[]; userClaims: string[]
}

export const apiResourcesApi = {
  list:   (params: string)                  => apiRequest<PagedResult<ApiResourceListItem>>(`/api/v1/api-resources${params}`),
  get:    (id: number)                      => apiRequest<ApiResourceDetails>(`/api/v1/api-resources/${id}`),
  create: (body: CreateApiResourceRequest)  => apiRequest<ApiResourceDetails>('/api/v1/api-resources', { method: 'POST', body: JSON.stringify(body) }),
  update: (id: number, body: Partial<CreateApiResourceRequest>) =>
    apiRequest<void>(`/api/v1/api-resources/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  delete: (id: number)                      => apiRequest<void>(`/api/v1/api-resources/${id}`, { method: 'DELETE' }),
  addScope:      (id: number, scope: string)   => apiRequest<void>(`/api/v1/api-resources/${id}/scopes`,     { method: 'POST', body: JSON.stringify({ scope }) }),
  removeScope:   (id: number, itemId: number)  => apiRequest<void>(`/api/v1/api-resources/${id}/scopes/${itemId}`,    { method: 'DELETE' }),
  addClaim:      (id: number, type: string)    => apiRequest<void>(`/api/v1/api-resources/${id}/claims`,     { method: 'POST', body: JSON.stringify({ type }) }),
  removeClaim:   (id: number, itemId: number)  => apiRequest<void>(`/api/v1/api-resources/${id}/claims/${itemId}`,    { method: 'DELETE' }),
  addSecret:     (id: number, body: object)    => apiRequest<{ id: number; created: string }>(`/api/v1/api-resources/${id}/secrets`, { method: 'POST', body: JSON.stringify(body) }),
  removeSecret:  (id: number, itemId: number)  => apiRequest<void>(`/api/v1/api-resources/${id}/secrets/${itemId}`,   { method: 'DELETE' }),
  addProperty:   (id: number, body: { key: string; value: string }) => apiRequest<void>(`/api/v1/api-resources/${id}/properties`, { method: 'POST', body: JSON.stringify(body) }),
  removeProperty:(id: number, itemId: number)  => apiRequest<void>(`/api/v1/api-resources/${id}/properties/${itemId}`,{ method: 'DELETE' }),
}
```

## Views

### `src/views/apiResources/ApiResourcesView.vue`

List page:
1. Heading "API Resources" + "New API Resource" button (opens `CreateApiResourceDialog`)
2. Search input (debounced)
3. `DataTable` columns: Enabled (badge), Name, Display Name, Discovery, Created, Actions ("View")
4. `PaginationBar`

### `src/views/apiResources/ApiResourceDetailView.vue`

Detail page with sections:

#### Basic Info
Fields: Enabled (toggle), Name (readonly), Display Name, Description,
Allowed Access Token Signing Algorithms (text input, e.g. `RS256`),
Show In Discovery Document (checkbox), Require Resource Indicator (checkbox).
"Save" → `PUT /api/v1/api-resources/{id}`.

#### Scopes
List of scope associations (scope name + delete button).
"Add Scope" input + button. These are names that must match existing `ApiScopes.Name` values —
optionally show a hint but do not enforce client-side.

#### User Claims
Tag list (claim type strings) with add/remove.

#### Secrets
Table: Type, Description, Expiration, Created, Delete button.
"Add Secret" Sheet: Value (required), Type (default "SharedSecret"), Description, Expiration.
Show one-time-only warning for the secret value.

#### Properties
Table: Key, Value, Delete. Inline add form.

#### Danger Zone
Delete button → `ConfirmDialog` → DELETE → navigate to list.

### `src/components/apiResources/CreateApiResourceDialog.vue`

Modal form:
- Name (required)
- Display Name
- Enabled (checkbox, default true)
- Scopes (tag input)
- User Claims (tag input)

On submit → `POST /api/v1/api-resources` → navigate to detail page.

## Reuse

Use the shared `DataTable`, `PaginationBar`, `ConfirmDialog`, and `usePagination` from Prompt 10.

## Acceptance Criteria

- List page loads API resources with pagination and search.
- Detail page displays all sections and saves correctly.
- Child collections (scopes, claims, secrets, properties) work with add/remove.
- Delete navigates back to list.
