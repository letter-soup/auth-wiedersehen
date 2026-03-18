# Auth.Wiedersehen — Web Client

Vue 3 single-page application that serves as the authentication UI for the Auth.Wiedersehen identity service. It
provides sign-in, sign-up, and password-reset flows with internationalisation support (English & Russian).

## Tech stack

| Category   | Technology                                                                                         |
|------------|----------------------------------------------------------------------------------------------------|
| Framework  | [Vue 3](https://vuejs.org/) (Composition API)                                                      |
| Build tool | [Vite](https://vitejs.dev/)                                                                        |
| Language   | TypeScript 5.8                                                                                     |
| Styling    | [Tailwind CSS 3](https://tailwindcss.com/) + [shadcn-vue](https://www.shadcn-vue.com/) (radix-vue) |
| State      | [Pinia 3](https://pinia.vuejs.org/)                                                                |
| Routing    | [Vue Router 4](https://router.vuejs.org/)                                                          |
| Forms      | [VeeValidate 4](https://vee-validate.logaretm.com/) + [Zod](https://zod.dev/)                      |
| i18n       | [vue-i18n 11](https://vue-i18n.intlify.dev/)                                                       |
| Unit tests | [Vitest](https://vitest.dev/) + `@vue/test-utils`                                                  |
| E2E tests  | [Playwright](https://playwright.dev/)                                                              |
| Linting    | ESLint 9 + [oxlint](https://oxc-project.github.io/docs/guide/usage/linter.html)                    |
| Formatting | [Prettier](https://prettier.io/)                                                                   |

## Prerequisites

- [Node.js LTS](https://nodejs.org/) (the Dockerfile uses `node:lts-alpine`)
- npm (ships with Node.js)

## Setup

```sh
npm install
```

## Development

```sh
npm run dev
```

Vite dev server starts on <http://localhost:5173> by default.

## Build

```sh
npm run build        # type-check + production build
npm run build-only   # production build without type-check
npm run type-check   # vue-tsc type checking only
```

## Testing

### Unit tests ([Vitest](https://vitest.dev/))

```sh
npm run test:unit          # single run
npm run test:unit:watch    # watch mode
npm run test:coverage      # with Istanbul coverage
```

### End-to-end tests ([Playwright](https://playwright.dev/))

```sh
# Install browsers (first time only)
npx playwright install

# Build the app first (required for CI)
npm run build

npm run test:e2e
npm run test:e2e -- --project=chromium   # Chromium only
npm run test:e2e -- --debug              # debug mode
```

## Linting & formatting

```sh
npm run lint           # runs oxlint then eslint (with --fix)
npm run lint:oxlint    # oxlint only
npm run lint:eslint    # eslint only
npm run format         # prettier — formats src/
```

## Environment configuration

Vite environment variables are defined via `.env` files at the web client root. Variables must be prefixed with `VITE_`
to be exposed to client code. See the [Vite env docs](https://vitejs.dev/guide/env-and-mode.html) for details.

## Project structure

```
src/
├── assets/            # Static assets & i18n JSON files (en-us, ru-ru)
├── components/
│   ├── layout/        # Layout components (footer, etc.)
│   ├── primitives/    # Reusable app-level components (language switcher, etc.)
│   └── ui/            # shadcn-vue primitives (button, card, form, input, label, popover, select, toast)
├── composable/        # Vue composables
├── lib/               # Utility functions
├── router/            # Vue Router configuration
└── views/             # Page-level components
    ├── SignInView/
    ├── SignUpView/
    └── ResetPasswordView.vue
e2e/                   # Playwright E2E specs
```

## Docker

The production image is a multi-stage build (`Dockerfile`):

1. **Builder** — `node:lts-alpine`, installs deps and runs `npm run build`
2. **Runtime** — `nginx:stable-alpine-slim`, serves the built SPA with the custom nginx config (`.nginx/nginx.conf`)