# Prompt 10 — UI: App Layout and Navigation

## Context

Continuing from Prompt 09. Login is working. We now build the persistent app shell used by all
authenticated pages: a sidebar with navigation links, a top header, and a main content area.

All protected views are wrapped in an `AppLayout` component.

## Files to Create / Modify

### `src/components/AppLayout.vue`

The root layout for authenticated pages. Uses a two-column grid: fixed sidebar + scrollable main.

Structure:
```
┌─────────────────────────────────────────────────────┐
│  Sidebar (fixed, 240px)  │  Top Header + Content    │
│                          │                           │
│  [Logo / Title]          │  [Breadcrumb] [User menu] │
│                          │  ─────────────────────── │
│  Navigation links:       │                           │
│  • Clients               │  <slot />                 │
│  • API Resources         │                           │
│  • API Scopes            │                           │
│  • Identity Resources    │                           │
│  • Identity Providers    │                           │
└──────────────────────────┴───────────────────────────┘
```

Use Tailwind CSS for layout (no CSS files):
- Outer wrapper: `flex h-screen overflow-hidden bg-background`
- Sidebar: `w-60 flex-shrink-0 border-r flex flex-col`
- Main: `flex flex-col flex-1 overflow-hidden`
- Header: `h-14 border-b flex items-center px-6 justify-between`
- Content: `flex-1 overflow-auto p-6`

### `src/components/AppSidebar.vue`

Sidebar component with:
- Brand section at top: icon + "Config Manager" title
- `<nav>` with `RouterLink` items, each highlighted when its route is active
- Navigation items:

```typescript
const navItems = [
  { name: 'Clients',             to: { name: 'clients' },             icon: 'MonitorSmartphone' },
  { name: 'API Resources',       to: { name: 'api-resources' },       icon: 'Server' },
  { name: 'API Scopes',          to: { name: 'api-scopes' },          icon: 'KeyRound' },
  { name: 'Identity Resources',  to: { name: 'identity-resources' },  icon: 'User' },
  { name: 'Identity Providers',  to: { name: 'identity-providers' },  icon: 'Globe' },
]
```

Use `lucide-vue-next` icons. Active state: darker background + text colour change.
Use shadcn-vue `Separator` between nav groups.

### `src/components/AppHeader.vue`

Thin top bar containing:
- Left: current route breadcrumb (route `meta.breadcrumb` string or route name)
- Right: `DropdownMenu` with `UserCircle` icon showing username from auth store
  - Menu item: "Sign out" → calls `authStore.logout()`

### `src/router/index.ts` — add all resource routes

Add routes for all five resource sections (views to be created in subsequent prompts).
Each route has `meta: { requiresAuth: true, breadcrumb: '<Section Name>' }`:

```typescript
{ path: '/clients',             name: 'clients',             component: () => import('@/views/clients/ClientsView.vue'),           meta: { requiresAuth: true, breadcrumb: 'Clients' } },
{ path: '/clients/:id',         name: 'client-detail',       component: () => import('@/views/clients/ClientDetailView.vue'),      meta: { requiresAuth: true, breadcrumb: 'Client Detail' } },
{ path: '/api-resources',       name: 'api-resources',       component: () => import('@/views/apiResources/ApiResourcesView.vue'), meta: { requiresAuth: true, breadcrumb: 'API Resources' } },
{ path: '/api-resources/:id',   name: 'api-resource-detail', component: () => import('@/views/apiResources/ApiResourceDetailView.vue'), meta: { requiresAuth: true, breadcrumb: 'API Resource Detail' } },
{ path: '/api-scopes',          name: 'api-scopes',          component: () => import('@/views/apiScopes/ApiScopesView.vue'),       meta: { requiresAuth: true, breadcrumb: 'API Scopes' } },
{ path: '/api-scopes/:id',      name: 'api-scope-detail',    component: () => import('@/views/apiScopes/ApiScopeDetailView.vue'),  meta: { requiresAuth: true, breadcrumb: 'API Scope Detail' } },
{ path: '/identity-resources',       name: 'identity-resources',       component: () => import('@/views/identityResources/IdentityResourcesView.vue'),       meta: { requiresAuth: true, breadcrumb: 'Identity Resources' } },
{ path: '/identity-resources/:id',   name: 'identity-resource-detail', component: () => import('@/views/identityResources/IdentityResourceDetailView.vue'),  meta: { requiresAuth: true, breadcrumb: 'Identity Resource Detail' } },
{ path: '/identity-providers',       name: 'identity-providers',       component: () => import('@/views/identityProviders/IdentityProvidersView.vue'),       meta: { requiresAuth: true, breadcrumb: 'Identity Providers' } },
{ path: '/identity-providers/:id',   name: 'identity-provider-detail', component: () => import('@/views/identityProviders/IdentityProviderDetailView.vue'),  meta: { requiresAuth: true, breadcrumb: 'Identity Provider Detail' } },
{ path: '/:pathMatch(.*)*', redirect: '/' },
```

Add the redirect: `{ path: '/', redirect: { name: 'clients' } }` so `/` goes to clients.

### `src/views/DashboardView.vue` — remove

No longer needed now that `/` redirects to `/clients`.

### Shared composable: `src/composables/usePagination.ts`

Reusable pagination state used by all list views:
```typescript
import { ref, computed } from 'vue'

export function usePagination(defaultPageSize = 20) {
  const page     = ref(1)
  const pageSize = ref(defaultPageSize)
  const search   = ref('')

  function reset() { page.value = 1 }

  const queryParams = computed(() =>
    `?page=${page.value}&pageSize=${pageSize.value}${search.value ? `&search=${encodeURIComponent(search.value)}` : ''}`
  )

  return { page, pageSize, search, queryParams, reset }
}
```

### Shared component: `src/components/DataTable.vue`

Generic table wrapper accepting `columns` and `rows` props (typed with generics) using shadcn-vue
`Table`, `TableHeader`, `TableRow`, `TableHead`, `TableBody`, `TableCell`. Supports a loading
skeleton and an empty-state slot.

### Shared component: `src/components/PaginationBar.vue`

Wraps shadcn-vue `Pagination` and emits `update:page`. Props: `totalCount`, `pageSize`, `page`.

### Shared component: `src/components/ConfirmDialog.vue`

Reusable delete-confirmation dialog using shadcn-vue `Dialog`. Props: `title`, `description`.
Emits `confirm` and `cancel`.

## Acceptance Criteria

- All authenticated views render inside `AppLayout`.
- Clicking a nav link highlights the correct sidebar item.
- Breadcrumb in header reflects the current section.
- Sign-out button in header dropdown works.
- Route guard still redirects unauthenticated users to `/login`.
- Stub `*View.vue` files exist for all resource routes so the router resolves without 404.
