# Auth.Wiedersehen

Authentication and identity management service
built with [Duende IdentityServer](https://duendesoftware.com/products/identityserver) on .NET 10 and a Vue 3 single-page frontend.

## Architecture

| Component                                     | Technology                                                                   | Port           |
|-----------------------------------------------|------------------------------------------------------------------------------|----------------|
| **API** (`Auth.Wiedersehen`)                  | ASP.NET Core 10, Duende IdentityServer 7, ASP.NET Identity, EF Core + Npgsql | `5002`         |
| **Web Client** (`Auth.Wiedersehen.WebClient`) | Vue 3, Vite, TypeScript, Tailwind CSS, shadcn-vue                            | `8080` (nginx) |
| **Seeder** (`Auth.Wiedersehen.Seeder`)        | .NET 10 console app — bootstraps dev data                                    | —              |
| **Database**                                  | PostgreSQL 17                                                                | `5432`         |

The backend exposes user registration, sign-in, email verification, and OAuth 2.0 / OpenID Connect endpoints. The frontend provides sign-in, sign-up, and reset-password views with i18n support (English & Russian).

### Database layout

The application uses three logical databases managed by EF Core migrations:

| Database             | EF Context                | Purpose                                            |
|----------------------|---------------------------|----------------------------------------------------|
| `auth_wiedersehen`   | `ApplicationDbContext`    | ASP.NET Identity users & roles                     |
| `configuration_db`   | `ConfigurationDbContext`  | IdentityServer clients, resources, scopes          |
| `persisted_grant_db` | `PersistedGrantDbContext` | IdentityServer operational data (tokens, consents) |

## Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Node.js LTS](https://nodejs.org/) (for the web client)
- [Docker & Docker Compose](https://docs.docker.com/engine/install/)
- [dingo](https://ujinjinjin.github.io/dingo/dingo.html#installation) — SQL migration runner
- GNU Make (ships with most Linux/macOS systems)

## Configure local environment

### 1. Start PostgreSQL

```shell
docker compose up -d auth-wiedersehen-db
```

The container exposes PostgreSQL on `localhost:5432`. The admin password is controlled by the `DB_ADMIN_PWD` environment variable (see [Environment variables](#environment-variables)).

### 2. Create a `.env` file

Create a `.env` file at the repository root (it is git-ignored):

```dotenv
DB_ADMIN_PWD=Qwert1234_

AW_APPLICATION_DB=Host=auth-wiedersehen-db;Database=auth_wiedersehen;Username=postgres;Password=Qwert1234_;
AW_CONFIGURATION_DB=Host=auth-wiedersehen-db;Database=configuration_db;Username=postgres;Password=Qwert1234_;
AW_PERSISTENT_GRAND_DB=Host=auth-wiedersehen-db;Database=persisted_grant_db;Username=postgres;Password=Qwert1234_;

AW_UI_HOST=http://localhost:8080
```

> When running the API **outside** Docker (self-hosted), replace `auth-wiedersehen-db` with `localhost` in the connection strings.

### 3. Apply EF Core migrations

From the `src/Auth.Wiedersehen/` directory:

```shell
make full_migrations
```

This bundles and runs migrations for all three databases. Individual targets are also available: `migrate_application`, `migrate_configuration`, `migrate_persistent_grant`.

### 4. Seed development data

```shell
docker compose up auth-wiedersehen-seeder
```

Or run the seeder directly:

```shell
cd src/Auth.Wiedersehen.Seeder
dotnet run -- /seed
```

## Run application

### Using Docker Compose (recommended)

```shell
docker compose up --build
```

| Service    | URL                     |
|------------|-------------------------|
| API        | <http://localhost:5002> |
| Web Client | <http://localhost:8080> |

### Self-hosted

**API:**

```shell
cd src/Auth.Wiedersehen
dotnet run
```

**Web Client:**

```shell
cd src/Auth.Wiedersehen.WebClient
npm install
npm run dev
```

The Vite dev server starts on <http://localhost:5173> by default.

## Running tests

### Backend

```shell
# Unit tests
cd src/Auth.Wiedersehen.UnitTests
dotnet test

# Integration tests (requires a running PostgreSQL instance)
cd src/Auth.Wiedersehen.IntegrationTests
dotnet test
```

### Frontend

```shell
cd src/Auth.Wiedersehen.WebClient

npm run test:unit          # Vitest — single run
npm run test:unit:watch    # Vitest — watch mode
npm run test:coverage      # Vitest with Istanbul coverage
npm run test:e2e           # Playwright end-to-end tests
```

Before running E2E tests for the first time:

```shell
npx playwright install
```

## Environment variables

| Variable                 | Description                                                 |
|--------------------------|-------------------------------------------------------------|
| `DB_ADMIN_PWD`           | PostgreSQL superuser password                               |
| `AW_APPLICATION_DB`      | Connection string — application (Identity) database         |
| `AW_CONFIGURATION_DB`    | Connection string — IdentityServer configuration database   |
| `AW_PERSISTENT_GRAND_DB` | Connection string — IdentityServer persisted grant database |
| `AW_UI_HOST`             | Base URL of the web client (used for redirect URLs)         |

## Branching model

| Branch      | Purpose                                             |
|-------------|-----------------------------------------------------|
| `main`      | Stable, production-ready code                       |
| `feature/*` | New features — branch off `main`, merge back via PR |
| `fix/*`     | Bug fixes — branch off `main`                       |

## CI / CD

GitHub Actions workflows are located in `.github/workflows/`:

- **auth-be-build-n-publish.yml** — builds and publishes the backend Docker image
- **auth-fe-build-n-publish.yml** — builds and publishes the frontend Docker image
- **codeql-analysis.yml** — CodeQL security scanning
