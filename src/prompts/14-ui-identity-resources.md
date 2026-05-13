# Prompt 14 — UI: Identity Resources Management

## Context

Continuing from Prompts 08–13. We now implement the Identity Resources management UI.

The Configuration Manager API (Prompt 06) exposes:
- `GET    /api/v1/identity-resources?page=&pageSize=&search=`
- `POST   /api/v1/identity-resources`
- `GET    /api/v1/identity-resources/{id}`
- `PUT    /api/v1/identity-resources/{id}`
- `DELETE /api/v1/identity-resources/{id}`
- Child collection endpoints: `/claims`, `/properties`

## API Client

### `src/api/identityResources.ts`

```typescript
import { apiRequest } from './client'
import type { PagedResult } from './clients'

export interface IdentityResourceListItem {
  id: number; name: string; displayName: string | null
  enabled: boolean; required: boolean; showInDiscoveryDocument: boolean; created: string
}

export interface IdentityResourceClaimItem    { id: number; type: string }
export interface IdentityResourcePropertyItem { id: number; key: string; value: string }

export interface IdentityResourceDetails {
  id: number; enabled: boolean; name: string; displayName: string | null
  description: string | null; required: boolean; emphasize: boolean
  showInDiscoveryDocument: boolean
  created: string; updated: string | null; nonEditable: boolean
  claims: IdentityResourceClaimItem[]; properties: IdentityResourcePropertyItem[]
}

export interface CreateIdentityResourceRequest {
  name: string; displayName?: string; description?: string
  enabled: boolean; required: boolean; emphasize: boolean
  showInDiscoveryDocument: boolean; userClaims: string[]
}

export const identityResourcesApi = {
  list:   (params: string)                      => apiRequest<PagedResult<IdentityResourceListItem>>(`/api/v1/identity-resources${params}`),
  get:    (id: number)                          => apiRequest<IdentityResourceDetails>(`/api/v1/identity-resources/${id}`),
  create: (body: CreateIdentityResourceRequest) => apiRequest<IdentityResourceDetails>('/api/v1/identity-resources', { method: 'POST', body: JSON.stringify(body) }),
  update: (id: number, body: Partial<CreateIdentityResourceRequest>) =>
    apiRequest<void>(`/api/v1/identity-resources/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  delete: (id: number)                          => apiRequest<void>(`/api/v1/identity-resources/${id}`, { method: 'DELETE' }),
  addClaim:      (id: number, type: string)    => apiRequest<void>(`/api/v1/identity-resources/${id}/claims`,     { method: 'POST', body: JSON.stringify({ type }) }),
  removeClaim:   (id: number, itemId: number)  => apiRequest<void>(`/api/v1/identity-resources/${id}/claims/${itemId}`,    { method: 'DELETE' }),
  addProperty:   (id: number, body: { key: string; value: string }) => apiRequest<void>(`/api/v1/identity-resources/${id}/properties`, { method: 'POST', body: JSON.stringify(body) }),
  removeProperty:(id: number, itemId: number)  => apiRequest<void>(`/api/v1/identity-resources/${id}/properties/${itemId}`,{ method: 'DELETE' }),
}
```

## Views

### `src/views/identityResources/IdentityResourcesView.vue`

List page:
1. Heading "Identity Resources" + "New Identity Resource" button
2. Search input (debounced)
3. `DataTable` columns: Enabled (badge), Name, Display Name, Required (badge), Discovery, Created, Actions
4. `PaginationBar`
5. Info callout: "Identity resources map scopes to user claims in the ID token. Standard resources:
   `openid` (sub claim), `profile`, `email`, `address`, `phone`."

### `src/views/identityResources/IdentityResourceDetailView.vue`

Detail page with sections:

#### Basic Info
Fields: Enabled (toggle), Name (readonly), Display Name, Description,
Required (checkbox), Emphasize (checkbox), Show In Discovery Document (checkbox).
"Save" → `PUT /api/v1/identity-resources/{id}`.

Note: if `nonEditable` is `true`, show a `Badge` labelled "System Resource" and disable all
save actions with a tooltip: "This resource is managed by IdentityServer and cannot be edited."

#### User Claims
Tag list with add/remove. Helper text: "Claims included in the ID token when this scope is
requested (e.g. `sub`, `name`, `email`)."

#### Properties
Table: Key, Value, Delete. Inline add form.

#### Danger Zone
Delete button (disabled + tooltip if `nonEditable`) → `ConfirmDialog` → DELETE → list.

### `src/components/identityResources/CreateIdentityResourceDialog.vue`

Modal form:
- Name (required)
- Display Name
- Enabled (checkbox, default true)
- Required (checkbox)
- Emphasize (checkbox)
- Show In Discovery Document (checkbox, default true)
- User Claims (tag input)

On submit → `POST /api/v1/identity-resources` → navigate to detail page.

## Notes

- Well-known identity resources (`openid`, `profile`, `email`) may have `nonEditable = true`
  set by Duende. Surface this clearly in the UI so users understand why editing is disabled.
- The concept explanation in the list page helps admins understand the difference between
  Identity Resources and API Resources/Scopes.

## Acceptance Criteria

- List loads with pagination and search.
- System resources show a "Non-editable" indicator and disable edit/delete.
- Detail page saves correctly for editable resources.
- Claims and properties manage correctly.
- Delete navigates back to list.
