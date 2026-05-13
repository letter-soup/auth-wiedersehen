# Prompt 09 — UI: Authentication (Login, Auth Store, Route Guards)

## Context

Continuing from Prompt 08. The project skeleton is in place at
`src/Auth.Wiedersehen.ConfigurationManager.WebClient/`.

The Configuration Manager API (Prompt 02) exposes `POST /api/v1/auth/token` which accepts
`{ username, password }` and returns `{ accessToken, expiresAt }`.

We need a login page, a Pinia auth store that persists the token in `localStorage`, and
route guards that enforce authentication.

## Files to Create / Modify

### `src/stores/auth.ts`

```typescript
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { apiRequest } from '@/api/client'
import router from '@/router'

interface TokenResponse {
  accessToken: string
  expiresAt: string
}

export const useAuthStore = defineStore('auth', () => {
  const token     = ref<string | null>(localStorage.getItem('cm_access_token'))
  const expiresAt = ref<string | null>(localStorage.getItem('cm_token_expires_at'))

  const isAuthenticated = computed(() => {
    if (!token.value || !expiresAt.value) return false
    return new Date(expiresAt.value) > new Date()
  })

  async function login(username: string, password: string) {
    const res = await apiRequest<TokenResponse>('/api/v1/auth/token', {
      method: 'POST',
      body: JSON.stringify({ username, password }),
    })
    token.value     = res.accessToken
    expiresAt.value = res.expiresAt
    localStorage.setItem('cm_access_token',    res.accessToken)
    localStorage.setItem('cm_token_expires_at', res.expiresAt)
    await router.push({ name: 'home' })
  }

  function logout() {
    token.value     = null
    expiresAt.value = null
    localStorage.removeItem('cm_access_token')
    localStorage.removeItem('cm_token_expires_at')
    router.push({ name: 'login' })
  }

  return { token, isAuthenticated, login, logout }
})
```

### `src/router/index.ts` — update guards

Replace the simple guard with one that also redirects authenticated users away from `/login`:

```typescript
router.beforeEach((to) => {
  const token     = localStorage.getItem('cm_access_token')
  const expiresAt = localStorage.getItem('cm_token_expires_at')
  const valid     = token && expiresAt && new Date(expiresAt) > new Date()

  if (to.meta.requiresAuth && !valid) return { name: 'login' }
  if (to.name === 'login' && valid)   return { name: 'home' }
})
```

### `src/views/LoginView.vue`

A centered card with:
- Application title: **"Configuration Manager"**
- `username` input (type text, shadcn-vue `Input`)
- `password` input (type password, shadcn-vue `Input`)
- Submit `Button` labelled "Sign In"
- Inline error message when login fails (display the API's error detail)
- Loading state on the button during the request

Use `vee-validate` + `zod` for form validation:
```typescript
const schema = z.object({
  username: z.string().min(1, 'Username is required'),
  password: z.string().min(1, 'Password is required'),
})
```

On submit, call `authStore.login(values.username, values.password)`.
Catch API errors and display `"Invalid credentials"` for `401` responses.

Use shadcn-vue components: `Card`, `CardHeader`, `CardTitle`, `CardContent`, `Input`, `Label`,
`Button`, `FormField`, `FormItem`, `FormLabel`, `FormControl`, `FormMessage`.

### `src/views/DashboardView.vue`

Placeholder page (filled properly in Prompt 10):
```vue
<template>
  <AppLayout>
    <h1 class="text-2xl font-semibold">Dashboard</h1>
    <p class="text-muted-foreground mt-1">Welcome to the Configuration Manager.</p>
  </AppLayout>
</template>
```

### `src/api/auth.ts`

Typed wrapper (optional, for clarity):
```typescript
import { apiRequest } from './client'

export interface LoginRequest { username: string; password: string }
export interface TokenResponse { accessToken: string; expiresAt: string }

export const authApi = {
  token: (req: LoginRequest) =>
    apiRequest<TokenResponse>('/api/v1/auth/token', {
      method: 'POST',
      body: JSON.stringify(req),
    }),
}
```

## UX Requirements

- The login card should be vertically and horizontally centred on the page with a subtle grey background.
- Show a spinner inside the Sign In button while the request is in flight.
- Focus the username field on mount (`autofocus` attribute).
- After successful login, the user is redirected to `/` (Dashboard).
- If the stored token is expired on any subsequent page load, the guard redirects to `/login` automatically.

## Acceptance Criteria

- Opening `http://localhost:8081/login` shows the login form.
- Correct credentials log in and navigate to the dashboard.
- Wrong credentials show an error message without navigating.
- Opening a protected route while unauthenticated redirects to `/login`.
- `localStorage` contains `cm_access_token` after login and is cleared on logout.
