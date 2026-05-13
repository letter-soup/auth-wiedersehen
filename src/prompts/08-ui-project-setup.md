# Prompt 08 — UI Project Setup: Auth.Wiedersehen.ConfigurationManager.WebClient

## Context

The repository already has a Vue 3 web client at `src/Auth.Wiedersehen.WebClient/`.
That project's `package.json` defines the canonical dependency versions to reuse.

We need a **second** Vue 3 SPA — the configuration manager UI — that:
- Uses **shadcn-vue** (https://www.shadcn-vue.com/) as its component library
- Runs on port **8081** in development and in Docker
- Connects to the Configuration Manager API at `http://localhost:5003`
- Shares the same major dependency versions as the existing web client

## Project Location

`src/Auth.Wiedersehen.ConfigurationManager.WebClient/`

## Tech Stack (versions must match Auth.Wiedersehen.WebClient's package.json)

| Package                        | Version from WebClient    |
|-------------------------------|---------------------------|
| `vue`                          | beta (same override)      |
| `vue-router`                   | ^5.0.4                    |
| `pinia`                        | ^3.0.4                    |
| `vite`                         | beta                      |
| `typescript`                   | ~6.0.2                    |
| `tailwindcss`                  | ^4.2.2                    |
| `@tailwindcss/vite`            | ^4.2.2                    |
| `reka-ui`                      | ^2.9.2  ← shadcn-vue peer |
| `class-variance-authority`     | ^0.7.1  ← shadcn-vue peer |
| `clsx`                         | ^2.1.1  ← shadcn-vue peer |
| `tailwind-merge`               | ^3.5.0  ← shadcn-vue peer |
| `lucide-vue-next`              | ^1.0.0                    |
| `@vueuse/core`                 | ^14.2.1                   |
| `vee-validate`                 | ^4.15.1                   |
| `@vee-validate/zod`            | ^4.15.1                   |
| `zod`                          | ^3.25.76                  |
| `vue-sonner`                   | ^2.0.9                    |

shadcn-vue scaffolds component files into the project; it is **not** an npm dependency itself.

## Files to Create

### `package.json`

```json
{
  "name": "auth-wiedersehen-cm",
  "version": "0.0.0",
  "private": true,
  "type": "module",
  "scripts": {
    "dev": "vite",
    "build": "run-p type-check \"build-only {@}\" --",
    "preview": "vite preview",
    "build-only": "vite build",
    "type-check": "vue-tsc --build",
    "lint": "eslint . --fix --cache"
  },
  "dependencies": {
    "@tailwindcss/vite": "^4.2.2",
    "@vee-validate/zod": "^4.15.1",
    "@vueuse/core": "^14.2.1",
    "class-variance-authority": "^0.7.1",
    "clsx": "^2.1.1",
    "lucide-vue-next": "^1.0.0",
    "pinia": "^3.0.4",
    "reka-ui": "^2.9.2",
    "tailwind-merge": "^3.5.0",
    "tailwindcss": "^4.2.2",
    "vee-validate": "^4.15.1",
    "vue": "beta",
    "vue-router": "^5.0.4",
    "vue-sonner": "^2.0.9",
    "zod": "^3.25.76"
  },
  "devDependencies": {
    "@tsconfig/node24": "^24.0.4",
    "@types/node": "^24.11.0",
    "@vitejs/plugin-vue": "^6.0.5",
    "@vue/eslint-config-typescript": "^14.7.0",
    "@vue/tsconfig": "^0.9.1",
    "eslint": "^10.1.0",
    "eslint-plugin-vue": "~10.8.0",
    "npm-run-all2": "^8.0.4",
    "typescript": "~6.0.2",
    "vite": "beta",
    "vue-tsc": "^3.2.6"
  },
  "engines": { "node": "^20.19.0 || >=22.12.0" },
  "overrides": {
    "vue": "beta",
    "@vue/compiler-core": "beta",
    "@vue/compiler-dom": "beta",
    "@vue/compiler-sfc": "beta",
    "@vue/compiler-ssr": "beta",
    "@vue/reactivity": "beta",
    "@vue/runtime-core": "beta",
    "@vue/runtime-dom": "beta",
    "@vue/server-renderer": "beta",
    "@vue/shared": "beta"
  }
}
```

### `vite.config.ts`

```typescript
import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [vue(), tailwindcss()],
  resolve: {
    alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) }
  },
  server: { port: 8081 }
})
```

### `tsconfig.json` / `tsconfig.app.json` / `tsconfig.node.json`

Follow the identical triple-config pattern from `src/Auth.Wiedersehen.WebClient/`.

### `src/main.ts`

```typescript
import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import './assets/main.css'

const app = createApp(App)
app.use(createPinia())
app.use(router)
app.mount('#app')
```

### `src/assets/main.css`

```css
@import 'tailwindcss';
```

### `src/lib/utils.ts`

Standard shadcn-vue utility:
```typescript
import { type ClassValue, clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}
```

### `src/App.vue`

Minimal root component — renders `<RouterView />` wrapped in a `<Toaster />` from `vue-sonner`.

### `src/router/index.ts`

Skeleton router with placeholder routes (to be filled in Prompts 09–15):
```typescript
import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/login',  name: 'login',  component: () => import('@/views/LoginView.vue') },
    { path: '/',       name: 'home',   component: () => import('@/views/DashboardView.vue'), meta: { requiresAuth: true } },
  ]
})

router.beforeEach((to) => {
  const token = localStorage.getItem('cm_access_token')
  if (to.meta.requiresAuth && !token) return { name: 'login' }
})

export default router
```

### `src/env.ts`

```typescript
export const API_BASE_URL = import.meta.env.VITE_API_URI ?? 'http://localhost:5003'
```

### `src/api/client.ts`

Thin fetch wrapper that injects the Bearer token:
```typescript
import { API_BASE_URL } from '@/env'

export async function apiRequest<T>(
  path: string,
  options: RequestInit = {}
): Promise<T> {
  const token = localStorage.getItem('cm_access_token')
  const res = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  })
  if (!res.ok) {
    const error = await res.json().catch(() => ({}))
    throw Object.assign(new Error(res.statusText), { status: res.status, data: error })
  }
  if (res.status === 204) return undefined as T
  return res.json()
}
```

### shadcn-vue components to scaffold

Run the following after `npm install` to add the required shadcn-vue components:
```bash
npx shadcn-vue@latest init
npx shadcn-vue@latest add button input label form table badge dialog sheet
npx shadcn-vue@latest add dropdown-menu separator card toast pagination
```

During `init`, choose:
- TypeScript: yes
- Tailwind CSS: yes (already configured)
- Components path: `src/components/ui`
- Utils path: `src/lib/utils`
- Style: Default (zinc base colour)

The `shadcn-vue add` commands scaffold component source files into `src/components/ui/`.

### `index.html`

Standard Vite `index.html` with `<div id="app"></div>` and `<script type="module" src="/src/main.ts">`.

### `.env.local` (gitignored)

```
VITE_API_URI=http://localhost:5003
```

### `Dockerfile`

```dockerfile
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
```

### `nginx.conf`

```nginx
server {
    listen 80;
    root /usr/share/nginx/html;
    index index.html;
    location / { try_files $uri $uri/ /index.html; }
    location /api/ {
        proxy_pass http://auth-wiedersehen-cm-api:5003;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

### `docker-compose.yml` — add service

```yaml
  auth-wiedersehen-cm-web:
    build:
      context: ./src/Auth.Wiedersehen.ConfigurationManager.WebClient
      dockerfile: ./Dockerfile
    depends_on:
      - auth-wiedersehen-cm-api
    ports:
      - "8081:80"
    environment:
      VITE_API_URI: ${AW_CM_API_URI:-http://localhost:5003}
```

## Acceptance Criteria

- `npm install && npm run dev` in the project directory starts on port 8081.
- `npm run build` completes without TypeScript or lint errors.
- The `src/components/ui/` directory contains the scaffolded shadcn-vue components.
- `src/lib/utils.ts` exports `cn`.
- The `apiRequest` helper correctly attaches the Bearer token from `localStorage`.
