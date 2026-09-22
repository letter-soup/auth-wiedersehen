# AGENTS.md — AI Agent Guidelines

This file provides context and conventions for AI agents (Junie, Copilot, Cursor, etc.) working on the Auth.Wiedersehen codebase.

## Project overview

Auth.Wiedersehen is an authentication / identity service. It consists of:

- **Backend API** (`src/Auth.Wiedersehen/`) — ASP.NET Core 10 web API using Duende IdentityServer 7, ASP.NET Identity, Entity Framework Core with PostgreSQL (Npgsql).
- **Web Client** (`src/Auth.Wiedersehen.WebClient/`) — Vue 3 SPA (Vite, TypeScript, Tailwind CSS, shadcn-vue, Pinia, vue-i18n).
- **Admin** (`src/Skoruba.Duende.IdentityServer.Admin/` + `.Admin.Api/`) — [skoruba/Duende.IdentityServer.Admin](https://github.com/skoruba/Duende.IdentityServer.Admin), for managing IdentityServer clients/resources and Identity users/roles; logs in via the backend API as its own IdentityServer client and reads/writes the same `configuration_db`/`persisted_grant_db`.
- **Tests** — Unit tests (`Auth.Wiedersehen.UnitTests`), integration tests (`Auth.Wiedersehen.IntegrationTests`), frontend unit tests (Vitest), and E2E tests (Playwright).

## Tech stack

| Layer              | Technologies                                    |
|--------------------|-------------------------------------------------|
| Backend runtime    | .NET 10, ASP.NET Core, C#                       |
| Identity           | Duende IdentityServer 7, ASP.NET Identity       |
| ORM / DB           | Entity Framework Core 10, Npgsql, PostgreSQL 17 |
| Validation         | FluentValidation 12                             |
| Logging            | Serilog                                         |
| Frontend framework | Vue 3 (Composition API), TypeScript 5.8         |
| Frontend build     | Vite 6                                          |
| Styling            | Tailwind CSS 3, shadcn-vue (radix-vue)          |
| State management   | Pinia 3                                         |
| Forms              | VeeValidate 4 + Zod                             |
| i18n               | vue-i18n 11 (en-us, ru-ru)                      |
| Frontend testing   | Vitest 3, @vue/test-utils, Playwright           |
| Linting            | ESLint 9, oxlint, Prettier                      |
| Containerisation   | Docker (multi-stage), Docker Compose, nginx     |
| CI                 | GitHub Actions                                  |

## Repository structure

```
/
├── .github/workflows/       # CI pipelines (build + publish, CodeQL)
├── docker-compose.yml        # Local dev orchestration (API, DB, web, admin, admin-api)
├── README.md                 # Root documentation
├── AGENTS.md                 # This file
├── src/
│   ├── Auth.Wiedersehen.slnx             # Solution file
│   ├── Auth.Wiedersehen/                  # Backend API project
│   │   ├── Authentication/                # IdentityServer auth config
│   │   ├── Configuration/                 # App configuration helpers
│   │   ├── Database/                      # DbContexts & EF migrations
│   │   ├── Emails/                        # Email controller & service
│   │   ├── Exceptions/                    # Custom exception types & filters
│   │   ├── Extensions/                    # Service/pipeline extension methods
│   │   ├── Localization/                  # Localizer abstraction
│   │   ├── Resources/                     # Shared .resx resources
│   │   ├── Users/                         # User controller, service, models
│   │   ├── Makefile                       # EF Core migration targets
│   │   ├── Dockerfile
│   │   └── Program.cs
│   ├── Auth.Wiedersehen.UnitTests/        # xUnit unit tests
│   ├── Auth.Wiedersehen.IntegrationTests/ # xUnit integration tests
│   ├── Auth.Wiedersehen.WebClient/        # Vue 3 SPA
│   ├── Skoruba.Duende.IdentityServer.Admin/       # Admin UI host
│   ├── Skoruba.Duende.IdentityServer.Admin.Api/   # Admin REST backend
│   └── Skoruba.Duende.IdentityServer.*             # Admin's EF Core/shared support projects
│       ├── src/
│       │   ├── assets/                    # CSS, i18n JSON
│       │   ├── components/                # layout, primitives, ui (shadcn)
│       │   ├── composable/                # Vue composables
│       │   ├── lib/                       # Utility functions
│       │   ├── router/                    # Vue Router config
│       │   └── views/                     # Page components
│       ├── e2e/                           # Playwright specs
│       ├── Dockerfile
│       └── package.json
```

## Coding conventions

### Backend (C#)

- Target framework: `net10.0`. Nullable reference types and implicit usings are enabled.
- Use **tabs** for indentation (see `.editorconfig`).
- Follow existing patterns: extension methods for service registration (`ConfigureServices`, `ConfigurePipeline`), feature folders (`Users/`, `Emails/`, `Authentication/`).
- Validation uses FluentValidation — add validators alongside the feature they validate.
- Logging via Serilog — use structured logging (`Log.Information("Message {Param}", value)`).
- Configuration is injected via `IOptions<T>` / `IConfiguration` with environment-variable prefixes (`AUTHW_` for the API, `AWA_` for the Admin UI/API).
- EF Core migrations live under `Database/Migrations/` with separate sub-folders per context (`ApplicationDb`, `ConfigurationDb`, `PersistedGrantDb`).
- Tests use xUnit with AutoFixture. Integration tests share a fixture (`IntegrationTestFixture`) and collection (`IntegrationTestsCollection`).

### Frontend (TypeScript / Vue)

- Use the **Composition API** with `<script setup lang="ts">`.
- Indentation: 2 spaces (see `.editorconfig` and `.prettierrc.json`).
- UI primitives come from **shadcn-vue** (`src/components/ui/`). Do not modify generated shadcn components unless necessary; create wrappers in `components/primitives/` instead.
- Translations are stored in `src/assets/i18n/{locale}.json`. Always add keys for both `en-us` and `ru-ru`.
- Form validation uses VeeValidate + Zod schemas.
- Unit tests sit next to the component they test (e.g., `FooterSection.spec.ts` beside `FooterSection.vue`) and use snapshot testing where appropriate.
- Run `npm run lint` and `npm run format` before committing frontend changes.

## Running locally

1. `docker compose up -d auth-wiedersehen-db` — start PostgreSQL.
2. Create a `.env` file at the repo root (see root `README.md` for template).
3. `cd src/Auth.Wiedersehen && make full_migrations` — apply EF Core migrations.
4. `docker compose up --build` — run all services, or start API and web client individually (see root `README.md`).

## Testing

- **Backend unit tests:** `cd src/Auth.Wiedersehen.UnitTests && dotnet test`
- **Backend integration tests:** `cd src/Auth.Wiedersehen.IntegrationTests && dotnet test` (needs running PostgreSQL)
- **Frontend unit tests:** `cd src/Auth.Wiedersehen.WebClient && npm run test:unit`
- **Frontend E2E tests:** `cd src/Auth.Wiedersehen.WebClient && npx playwright install && npm run test:e2e`

## Important notes for agents

- The solution file uses the `.slnx` format (XML-based, lightweight).
- The backend publishes as a **self-contained single-file** executable targeting `linux-musl-x64` (Alpine).
- Three separate PostgreSQL databases are used — do not assume a single database.
- Environment variables use prefixes: `AUTHW_` (API), `AWA_` (Admin UI/API). Nested keys use `__` as separator (e.g., `AUTHW_ConnectionStrings__ApplicationDB`).
- The frontend Docker image serves via **nginx** — the config is at `.nginx/nginx.conf`.
- When adding new API endpoints, follow the existing controller pattern in feature folders and register routes in the pipeline extensions.
- When adding new frontend pages, add the route in `src/router/index.ts` and create a view folder under `src/views/`.
