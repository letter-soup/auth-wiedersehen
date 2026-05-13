# Prompt 01 — API Project Setup: Auth.Wiedersehen.ConfigurationManager.Api

## Context

This repository contains `Auth.Wiedersehen` — an ASP.NET Core 10 / Duende IdentityServer 7 auth server.
The solution file is at `src/Auth.Wiedersehen.slnx`.
The main API lives at `src/Auth.Wiedersehen/` and runs on port **5002**.
Infrastructure is managed by `docker-compose.yml` at the repo root.

**Prerequisite:** Prompt 00 must be completed first. It creates `Auth.Wiedersehen.Shared` — the
class library that holds exceptions, guard-clause helpers, and validation extensions reused by
both the main API and this new project.

The main API's `Auth.Wiedersehen.csproj` uses these package versions (treat as canonical — match them):
- `Duende.IdentityServer.EntityFramework` 7.4.7
- `FluentValidation` 12.1.1
- `Microsoft.AspNetCore.OpenApi` 10.0.5
- `Microsoft.EntityFrameworkCore.Design` 10.0.5
- `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.1
- `Serilog.AspNetCore` 10.0.0
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 10.0.5

## Goal

Scaffold a **new ASP.NET Core Web API project** named `Auth.Wiedersehen.ConfigurationManager.Api` that:
- Targets `.NET 10.0`
- Shares the same package versions as the main API
- Mirrors the main API's code organisation (`Extensions/`, `Configuration/`, `Exceptions/`, etc.)
- Connects to two PostgreSQL databases: the existing Duende **ConfigurationDB** (read/write) and a new **ManagerDB** (for internal auth — added in Prompt 02)
- Runs on port **5003**
- Is added to `src/Auth.Wiedersehen.slnx`
- Has its own `Dockerfile` and a new `docker-compose.yml` service

## Files to Create / Modify

### 1. `src/Auth.Wiedersehen.ConfigurationManager.Api/Auth.Wiedersehen.ConfigurationManager.Api.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <PublishSingleFile>true</PublishSingleFile>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Auth.Wiedersehen.Shared\Auth.Wiedersehen.Shared.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Duende.IdentityServer.EntityFramework" Version="7.4.7" />
    <PackageReference Include="FluentValidation" Version="12.1.1" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.5" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.5" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.5">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.1" />
    <PackageReference Include="Serilog.AspNetCore" Version="10.0.0" />
  </ItemGroup>
</Project>
```

> Note: `Microsoft.AspNetCore.Identity.EntityFrameworkCore` is intentionally absent — this project
> has no ASP.NET Identity dependency. `FluentValidation` is kept because the CM API has its own
> validators; the shared library also uses it but doesn't re-export validators.

### 2. `src/Auth.Wiedersehen.ConfigurationManager.Api/Program.cs`

Follow the identical pattern used in `src/Auth.Wiedersehen/Program.cs`:
- `Log.Logger` bootstrap with Serilog
- `builder.ConfigureLogging()`
- `builder.ConfigureServices()`
- `app.ConfigurePipeline()`

### 3. `src/Auth.Wiedersehen.ConfigurationManager.Api/Extensions/WebAppExtensions.cs`

Three extension methods:
- `ConfigureLogging` — Serilog console output, same template as main API
- `ConfigureServices` — registers DbContexts, controllers (with `options.Filters.Add<HttpResponseExceptionFilter>()`),
  OpenApi, authentication (JWT — see Prompt 02), FluentValidation validators.
  `HttpResponseExceptionFilter` comes from `Auth.Wiedersehen.Shared.Exceptions`.
- `ConfigurePipeline` — `UseSerilogRequestLogging`, `UseAuthentication`, `UseAuthorization`, `MapControllers`, `MapOpenApi`, dev exception page

### 4. `src/Auth.Wiedersehen.ConfigurationManager.Api/Configuration/ConfigurationKey.cs`

```csharp
namespace AuthCM.Configuration;

internal struct ConfigurationKey
{
    public struct ConnectionString
    {
        public const string ConfigurationDb = "ConfigurationDB";
        public const string ManagerDb       = "ManagerDB";
    }

    public struct Jwt
    {
        public const string SigningKey     = "Jwt:SigningKey";
        public const string Issuer        = "Jwt:Issuer";
        public const string Audience      = "Jwt:Audience";
        public const string ExpiryMinutes = "Jwt:ExpiryMinutes";
    }

    public struct Admin
    {
        public const string Username = "Admin:Username";
        public const string Password = "Admin:Password";
    }
}
```

### 5. `src/Auth.Wiedersehen.ConfigurationManager.Api/Configuration/ConfigurationExtensions.cs`

Identical pattern to `src/Auth.Wiedersehen/Configuration/ConfigurationExtensions.cs`:
load JSON then `AddEnvironmentVariables("AWCM_")`.

### 6. `src/Auth.Wiedersehen.ConfigurationManager.Api/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" }
  },
  "Jwt": {
    "Issuer": "auth-wiedersehen-cm",
    "Audience": "auth-wiedersehen-cm-ui",
    "ExpiryMinutes": 60
  }
}
```

### 7. `src/Auth.Wiedersehen.ConfigurationManager.Api/appsettings.Development.json`

```json
{
  "Logging": { "LogLevel": { "Default": "Debug", "Microsoft.AspNetCore": "Information" } }
}
```

### 8. Shared utilities — no local copies needed

All of the following come from `Auth.Wiedersehen.Shared` (added via the `ProjectReference` above).
**Do not** create local copies in this project.

| Type | Namespace in the shared lib |
|------|-----------------------------|
| `HttpResponseException` | `Auth.Wiedersehen.Shared.Exceptions` |
| `HttpResponseExceptionFilter` | `Auth.Wiedersehen.Shared.Exceptions` |
| `ErrorDetails` | `Auth.Wiedersehen.Shared.Exceptions` |
| `ObjectExtensions` (`Required<T>`) | `Auth.Wiedersehen.Shared.Extensions` |
| `ValidationResultExtensions` | `Auth.Wiedersehen.Shared.Extensions` |
| `StringExtensions` | `Auth.Wiedersehen.Shared.Extensions` |

Use these with the appropriate `using` directives throughout this project:
```csharp
using Auth.Wiedersehen.Shared.Exceptions;
using Auth.Wiedersehen.Shared.Extensions;
```

### 9. `src/Auth.Wiedersehen.ConfigurationManager.Api/Dockerfile`

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Auth.Wiedersehen.ConfigurationManager.Api.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION --self-contained --runtime linux-musl-x64 -o /app/publish

FROM alpine:latest AS final
RUN apk add --no-cache libgcc libstdc++ icu-libs
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./Auth.Wiedersehen.ConfigurationManager.Api"]
```

### 10. `src/Auth.Wiedersehen.slnx` — add the new project

Add inside `<Solution>`:
```xml
<Project Path="Auth.Wiedersehen.ConfigurationManager.Api/Auth.Wiedersehen.ConfigurationManager.Api.csproj" />
```

### 11. `docker-compose.yml` — add new service

```yaml
  auth-wiedersehen-cm-api:
    build:
      context: ./src/Auth.Wiedersehen.ConfigurationManager.Api
      dockerfile: ./Dockerfile
    ports:
      - '5003:5003'
    depends_on:
      - auth-wiedersehen-db
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: "http://0.0.0.0:5003"
      AWCM_ConnectionStrings__ConfigurationDB: ${AW_CONFIGURATION_DB}
      AWCM_ConnectionStrings__ManagerDB: ${AW_MANAGER_DB}
      AWCM_Jwt__SigningKey: ${AW_CM_JWT_SIGNING_KEY}
      AWCM_Admin__Username: ${AW_CM_ADMIN_USERNAME}
      AWCM_Admin__Password: ${AW_CM_ADMIN_PASSWORD}
```

### 12. `.env` — add new variables

```
AW_MANAGER_DB=Host=auth-wiedersehen-db;Port=5432;Database=aw-configuration-manager;Username=postgres;Password=Qwert1234_
AW_CM_JWT_SIGNING_KEY=change-me-to-a-long-random-secret-at-least-32-chars
AW_CM_ADMIN_USERNAME=admin
AW_CM_ADMIN_PASSWORD=Admin1234_
```

## Acceptance Criteria

- `dotnet build src/Auth.Wiedersehen.slnx` succeeds with the new project included.
- `dotnet run --project src/Auth.Wiedersehen.ConfigurationManager.Api` starts on port 5003.
- `/openapi/v1.json` is reachable at runtime.
- All namespaces use `AuthCM` as the root (short alias to avoid verbosity).
