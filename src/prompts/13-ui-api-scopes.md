# Prompt 13 — UI: API Scopes Management

## Context

Continuing from Prompts 08–12. We now implement the API Scopes management UI.

The Configuration Manager API (Prompt 05) exposes:
- `GET    /api/v1/api-scopes?page=&pageSize=&search=`
- `POST   /api/v1/api-scopes`
- `GET    /api/v1/api-scopes/{id}`
- `PUT    /api/v1/api-scopes/{id}`
- `DELETE /api/v1/api-scopes/{id}`
- Child collection endpoints: `/claims`, `/properties`

## API Client

### `src/api/apiScopes.ts`

```typescript
import { apiRequest } from './client'
import type { PagedResult } from './clients'

export interface ApiScopeListItem {
  id: number; name: string; displayName: string | null
  enabled: boolean; required: boolean; showInDiscoveryDocument: boolean; created: string
}

export interface ApiScopeClaimItem    { id: number; type: string }
export interface ApiScopePropertyItem { id: number; key: string; value: string }

export interface ApiScopeDetails {
  id: number; enabled: boolean; name: string; displayName: string | null
  description: string | null; required: boolean; emphasize: boolean
  showInDiscoveryDocument: boolean
  created: string; updated: string | null; lastAccessed: string | null; nonEditable: boolean
  claims: ApiScopeClaimItem[]; properties: ApiScopePropertyItem[]
}

export interface CreateApiScopeRequest {
  name: string; displayName?: string; description?: string
  enabled: boolean; required: boolean; emphasize: boolean
  showInDiscoveryDocument: boolean; userClaims: string[]
}

export const apiScopesApi = {
  list:   (params: string)                => apiRequest<PagedResult<ApiScopeListItem>>(`/api/v1/api-scopes${params}`),
  get:    (id: number)                    => apiRequest<ApiScopeDetails>(`/api/v1/api-scopes/${id}`),
  create: (body: CreateApiScopeRequest)   => apiRequest<ApiScopeDetails>('/api/v1/api-scopes', { method: 'POST', body: JSON.stringify(body) }),
  update: (id: number, body: Partial<CreateApiScopeRequest>) =>
    apiRequest<void>(`/api/v1/api-scopes/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  delete: (id: number)                    => apiRequest<void>(`/api/v1/api-scopes/${id}`, { method: 'DELETE' }),
  addClaim:      (id: number, type: string)    => apiRequest<void>(`/api/v1/api-scopes/${id}/claims`,     { method: 'POST', body: JSON.stringify({ type }) }),
  removeClaim:   (id: number, itemId: number)  => apiRequest<void>(`/api/v1/api-scopes/${id}/claims/${itemId}`,    { method: 'DELETE' }),
  addProperty:   (id: number, body: { key: string; value: string }) => apiRequest<void>(`/api/v1/api-scopes/${id}/properties`, { method: 'POST', body: JSON.stringify(body) }),
  removeProperty:(id: number, itemId: number)  => apiRequest<void>(`/api/v1/api-scopes/${id}/properties/${itemId}`,{ method: 'DELETE' }),
}
```

## Views

### `src/views/apiScopes/ApiScopesView.vue`

List page:
1. Heading "API Scopes" + "New API Scope" button (opens `CreateApiScopeDialog`)
2. Search input (debounced)
3. `DataTable` columns: Enabled (badge), Name, Display Name, Required (badge), Discovery, Created, Actions
4. `PaginationBar`

### `src/views/apiScopes/ApiScopeDetailView.vue`

Detail page with sections:

#### Basic Info
Fields: Enabled (toggle), Name (readonly), Display Name, Description,
Required (checkbox), Emphasize (checkbox), Show In Discovery Document (checkbox).
"Save" → `PUT /api/v1/api-scopes/{id}`.

#### User Claims
Tag list (claim type strings) with add/remove.
Each item: badge showing claim type + remove X button.

#### Properties
Table: Key, Value, Delete. Inline add form (Key + Value inputs + "Add" button).

#### Danger Zone
Delete button → `ConfirmDialog` → DELETE → navigate to list.

### `src/components/apiScopes/CreateApiScopeDialog.vue`

Modal form:
- Name (required)
- Display Name
- Enabled (checkbox, default true)
- Required (checkbox)
- Emphasize (checkbox)
- Show In Discovery Document (checkbox, default true)
- User Claims (tag input — type and press Enter to add)

On submit → `POST /api/v1/api-scopes` → navigate to detail page.

## Notes

- API Scopes are the permissions that clients request. They bridge Clients (`ClientScopes`)
  and API Resources (`ApiResourceScopes`). The UI should communicate this concept clearly
  with helper text: "Scopes defined here can be assigned to clients and API resources."
- The `Emphasize` flag makes the scope stand out on the consent screen.
- The `Required` flag means the scope is always included regardless of consent.

## Acceptance Criteria

- List page loads with pagination and search.
- Detail page displays all fields and saves correctly.
- Claims and properties child collections work.
- Delete navigates back to list.
