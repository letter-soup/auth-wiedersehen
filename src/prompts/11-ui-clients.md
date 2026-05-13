# Prompt 11 — UI: Clients Management

## Context

Continuing from Prompt 10. The app shell is in place. We now implement the full Clients
management UI: a list page and a detail/edit page.

The Configuration Manager API (Prompt 03) exposes these endpoints:
- `GET    /api/v1/clients?page=1&pageSize=20&search=`
- `POST   /api/v1/clients`
- `GET    /api/v1/clients/{id}`
- `PUT    /api/v1/clients/{id}`
- `DELETE /api/v1/clients/{id}`
- Child collection endpoints per Prompt 03

## API Client

### `src/api/clients.ts`

Create a typed API module mirroring the server's DTOs:

```typescript
import { apiRequest } from './client'

// --- list types ---
export interface ClientListItem {
  id: number; clientId: string; clientName: string | null
  enabled: boolean; protocolType: string; created: string
}
export interface PagedResult<T> { items: T[]; totalCount: number; page: number; pageSize: number }

// --- detail types ---
export interface ClientSecretItem { id: number; description: string | null; type: string; value: string; expiration: string | null; created: string }
export interface ClientClaimItem  { id: number; type: string; value: string }
export interface ClientPropertyItem { id: number; key: string; value: string }
export interface ClientDetails {
  id: number; enabled: boolean; clientId: string; protocolType: string
  requireClientSecret: boolean; clientName: string | null; description: string | null
  clientUri: string | null; logoUri: string | null; requireConsent: boolean
  allowRememberConsent: boolean; alwaysIncludeUserClaimsInIdToken: boolean
  requirePkce: boolean; allowPlainTextPkce: boolean; requireRequestObject: boolean
  allowAccessTokensViaBrowser: boolean; requireDPoP: boolean
  frontChannelLogoutUri: string | null; frontChannelLogoutSessionRequired: boolean
  backChannelLogoutUri: string | null; backChannelLogoutSessionRequired: boolean
  allowOfflineAccess: boolean; identityTokenLifetime: number; accessTokenLifetime: number
  authorizationCodeLifetime: number; consentLifetime: number | null
  absoluteRefreshTokenLifetime: number; slidingRefreshTokenLifetime: number
  refreshTokenUsage: number; updateAccessTokenClaimsOnRefresh: boolean
  refreshTokenExpiration: number; accessTokenType: number; enableLocalLogin: boolean
  includeJwtId: boolean; alwaysSendClientClaims: boolean
  clientClaimsPrefix: string | null; pairWiseSubjectSalt: string | null
  initiateLoginUri: string | null; userSsoLifetime: number | null
  userCodeType: string | null; deviceCodeLifetime: number
  cibaLifetime: number | null; pollingInterval: number | null
  coordinateLifetimeWithUserSession: boolean | null
  requirePushedAuthorization: boolean; pushedAuthorizationLifetime: number | null
  created: string; updated: string | null; lastAccessed: string | null; nonEditable: boolean
  secrets: ClientSecretItem[]; scopes: string[]; redirectUris: string[]
  postLogoutRedirectUris: string[]; corsOrigins: string[]; grantTypes: string[]
  claims: ClientClaimItem[]; properties: ClientPropertyItem[]; idpRestrictions: string[]
}

export interface CreateClientRequest {
  clientId: string; protocolType: string; requireClientSecret: boolean
  clientName?: string; description?: string; requireConsent: boolean
  allowRememberConsent: boolean; requirePkce: boolean; allowOfflineAccess: boolean
  enableLocalLogin: boolean; allowedGrantTypes: string[]; redirectUris: string[]
  postLogoutRedirectUris: string[]; allowedScopes: string[]; allowedCorsOrigins: string[]
}

export const clientsApi = {
  list:   (params: string)        => apiRequest<PagedResult<ClientListItem>>(`/api/v1/clients${params}`),
  get:    (id: number)            => apiRequest<ClientDetails>(`/api/v1/clients/${id}`),
  create: (body: CreateClientRequest) => apiRequest<ClientDetails>('/api/v1/clients', { method: 'POST', body: JSON.stringify(body) }),
  update: (id: number, body: Partial<CreateClientRequest>) =>
                                     apiRequest<void>(`/api/v1/clients/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  delete: (id: number)            => apiRequest<void>(`/api/v1/clients/${id}`, { method: 'DELETE' }),
  // child collections
  addScope:      (id: number, scope: string)           => apiRequest<void>(`/api/v1/clients/${id}/scopes`,      { method: 'POST', body: JSON.stringify({ scope }) }),
  removeScope:   (id: number, itemId: number)          => apiRequest<void>(`/api/v1/clients/${id}/scopes/${itemId}`, { method: 'DELETE' }),
  addGrantType:  (id: number, grantType: string)       => apiRequest<void>(`/api/v1/clients/${id}/grant-types`, { method: 'POST', body: JSON.stringify({ grantType }) }),
  removeGrantType:(id: number, itemId: number)         => apiRequest<void>(`/api/v1/clients/${id}/grant-types/${itemId}`, { method: 'DELETE' }),
  addRedirectUri:(id: number, uri: string)             => apiRequest<void>(`/api/v1/clients/${id}/redirect-uris`, { method: 'POST', body: JSON.stringify({ redirectUri: uri }) }),
  removeRedirectUri:(id: number, itemId: number)       => apiRequest<void>(`/api/v1/clients/${id}/redirect-uris/${itemId}`, { method: 'DELETE' }),
  addCorsOrigin: (id: number, origin: string)          => apiRequest<void>(`/api/v1/clients/${id}/cors-origins`, { method: 'POST', body: JSON.stringify({ origin }) }),
  removeCorsOrigin:(id: number, itemId: number)        => apiRequest<void>(`/api/v1/clients/${id}/cors-origins/${itemId}`, { method: 'DELETE' }),
  addSecret:     (id: number, body: object)            => apiRequest<{ id: number; created: string }>(`/api/v1/clients/${id}/secrets`, { method: 'POST', body: JSON.stringify(body) }),
  removeSecret:  (id: number, itemId: number)          => apiRequest<void>(`/api/v1/clients/${id}/secrets/${itemId}`, { method: 'DELETE' }),
  addClaim:      (id: number, body: { type: string; value: string }) => apiRequest<void>(`/api/v1/clients/${id}/claims`, { method: 'POST', body: JSON.stringify(body) }),
  removeClaim:   (id: number, itemId: number)          => apiRequest<void>(`/api/v1/clients/${id}/claims/${itemId}`, { method: 'DELETE' }),
  addProperty:   (id: number, body: { key: string; value: string }) => apiRequest<void>(`/api/v1/clients/${id}/properties`, { method: 'POST', body: JSON.stringify(body) }),
  removeProperty:(id: number, itemId: number)          => apiRequest<void>(`/api/v1/clients/${id}/properties/${itemId}`, { method: 'DELETE' }),
}
```

## Views

### `src/views/clients/ClientsView.vue`

List page:
1. Heading "Clients" + "New Client" `Button` (opens `CreateClientDialog`)
2. Search `Input` bound to `usePagination().search` (debounced 300ms)
3. `DataTable` with columns: Enabled (badge), Client ID, Name, Protocol, Created, Actions
4. Actions column: "View" button → navigate to `{ name: 'client-detail', params: { id } }`
5. `PaginationBar` at the bottom
6. Loading skeleton while fetching

### `src/views/clients/ClientDetailView.vue`

Detail/edit page for a single client. Layout: tabs or sections.

**Sections:**

#### Basic Info (always visible, editable)
Fields: Enabled (toggle), Client ID (readonly), Name, Description, Protocol Type, Client URI,
Logo URI, Require Consent, Allow Remember Consent.
"Save" button → `PUT /api/v1/clients/{id}`.

#### Lifetimes & Token Settings (collapsible or tab)
Fields (all numeric inputs): Identity Token Lifetime, Access Token Lifetime, Authorization Code
Lifetime, Consent Lifetime, Absolute Refresh Token Lifetime, Sliding Refresh Token Lifetime,
Device Code Lifetime. Plus: Access Token Type (select: JWT=0, Reference=1),
Refresh Token Usage (select: ReUse=0, OneTimeOnly=1),
Refresh Token Expiration (select: Sliding=0, Absolute=1).

#### Grant Types
Inline tag list + "Add" input + remove badge button.
Common grant type suggestions: `authorization_code`, `client_credentials`, `implicit`,
`hybrid`, `device_flow`, `urn:ietf:params:oauth:grant-type:ciba`.

#### Redirect URIs / Post-Logout Redirect URIs / CORS Origins
Each as a simple list with:
- Inline text showing the URI
- Delete icon button to remove
- "Add" input + button at the bottom

#### Allowed Scopes
Tag list with add/remove, same pattern.

#### Secrets
Table: Type, Description, Expiration, Created, Delete button.
"Add Secret" button opens a `Sheet` (slide-over) with Value, Type, Description, Expiration fields.
Remind the user the value is shown only once.

#### Claims & Properties
Two sub-tables: Claims (Type, Value, Delete) and Properties (Key, Value, Delete).
Both with inline add forms.

#### IDP Restrictions
Tag list with add/remove.

#### Danger Zone
"Delete Client" button → `ConfirmDialog` → DELETE then navigate back to list.

### `src/components/clients/CreateClientDialog.vue`

Modal `Dialog` with a minimal create form:
- Client ID (required)
- Client Name
- Protocol Type (select: `oidc`, `wsfed`)
- Require Client Secret (checkbox)
- At least one Grant Type (multi-select or tag input, required)
- Redirect URIs (textarea, one per line)
- Allowed Scopes (tag input)

On submit → `POST /api/v1/clients` → navigate to the new client's detail page.

## Toasts

Use `vue-sonner`'s `toast` function:
- Success: "Client created", "Changes saved", "Client deleted"
- Error: display the API error message

## Acceptance Criteria

- List loads with pagination and search.
- Creating a client navigates to its detail page.
- All sections in the detail page save correctly to the API.
- Child collection add/remove operations update the UI without full page reload.
- Delete from detail page removes the client and returns to the list.
