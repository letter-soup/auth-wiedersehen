# Prompt 02 — API Authentication: Internal JWT Auth with DB-backed Credentials

## Context

Continuing from Prompt 01. The project `Auth.Wiedersehen.ConfigurationManager.Api` now exists at
`src/Auth.Wiedersehen.ConfigurationManager.Api/` but has no authentication yet.

The API must expose a single **token endpoint** for admin login. All other endpoints will require
a valid JWT. Admin credentials are seeded once from environment variables (`AWCM_Admin__Username`
and `AWCM_Admin__Password`) into a dedicated PostgreSQL database (`ManagerDB`) so they can later
be changed without a redeploy.

The existing configuration database (`ConfigurationDB`) must **not** be modified — the ManagerDB
is a separate database on the same PostgreSQL instance.

## Database — ManagerDbContext

### Entity

```
src/Auth.Wiedersehen.ConfigurationManager.Api/Database/AdminUser.cs
```

```csharp
namespace AuthCM.Database;

public sealed class AdminUser
{
    public int    Id           { get; set; }
    public string Username     { get; set; } = string.Empty;  // varchar 200, unique
    public string PasswordHash { get; set; } = string.Empty;  // BCrypt hash
    public bool   IsActive     { get; set; } = true;
    public DateTime CreatedAt  { get; set; }
    public DateTime UpdatedAt  { get; set; }
}
```

### DbContext

```
src/Auth.Wiedersehen.ConfigurationManager.Api/Database/ManagerDbContext.cs
```

```csharp
namespace AuthCM.Database;

public sealed class ManagerDbContext(DbContextOptions<ManagerDbContext> options) : DbContext(options)
{
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<AdminUser>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Username).HasMaxLength(200).IsRequired();
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
        });
    }
}
```

### Migration

Generate an EF Core migration named `InitManagerDb` for `ManagerDbContext` that creates the
`AdminUsers` table. Store migrations under:
```
src/Auth.Wiedersehen.ConfigurationManager.Api/Database/Migrations/ManagerDb/
```

Add the migration assembly path to `ManagerDbContext` options:
```csharp
options.UseNpgsql(connectionString,
    sql => sql.MigrationsAssembly(typeof(Program).Assembly.GetName().Name));
```

Apply migrations automatically on startup (call `context.Database.MigrateAsync()` in a hosted
service or during app startup before `app.Run()`).

### Seeder

```
src/Auth.Wiedersehen.ConfigurationManager.Api/Database/AdminUserSeeder.cs
```

On startup, if `AdminUsers` table is empty, create one admin user using credentials from:
- `ConfigurationKey.Admin.Username` (`AWCM_Admin__Username`)
- `ConfigurationKey.Admin.Password` (`AWCM_Admin__Password`)

Hash the password with `BCrypt.Net.BCrypt.HashPassword(password)`. Add the BCrypt NuGet package:
```xml
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
```

## Auth Endpoint

### Models

```
src/Auth.Wiedersehen.ConfigurationManager.Api/Auth/AuthModels.cs
```

```csharp
namespace AuthCM.Auth;

public sealed record LoginRequest(string Username, string Password);
public sealed record TokenResponse(string AccessToken, DateTimeOffset ExpiresAt);
```

### Controller

```
src/Auth.Wiedersehen.ConfigurationManager.Api/Auth/AuthController.cs
```

Route: `POST /api/v1/auth/token`
- No `[Authorize]` attribute — this is the public login endpoint.
- Validate `LoginRequest` with FluentValidation (Username and Password are required, max 200 chars).
- Lookup admin user by username in `ManagerDbContext`.
- Verify password with `BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)`.
- On failure → `401 Unauthorized` with `ErrorDetails`.
- On success → call `IIssueTokenCommand` and return `200 OK` with `TokenResponse`.

### Command

```
src/Auth.Wiedersehen.ConfigurationManager.Api/Auth/Commands/IssueTokenCommand.cs
```

Generates a JWT using `System.IdentityModel.Tokens.Jwt`:
- Claims: `sub` = user.Id.ToString(), `name` = user.Username, `jti` = Guid.NewGuid()
- Signed with `HmacSha256` using the key from `ConfigurationKey.Jwt.SigningKey`
- Issuer, audience, and expiry from configuration
- Returns `TokenResponse(token, expiresAt)`

## JWT Middleware Registration

In `WebAppExtensions.ConfigureServices`, add:

```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidIssuer              = builder.Configuration[ConfigurationKey.Jwt.Issuer],
            ValidateAudience         = true,
            ValidAudience            = builder.Configuration[ConfigurationKey.Jwt.Audience],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration[ConfigurationKey.Jwt.SigningKey]!)),
            ValidateLifetime         = true,
        };
    });

builder.Services.AddAuthorization();
```

In `ConfigurePipeline`:
```csharp
app.UseAuthentication();
app.UseAuthorization();
```

All controllers that are added in subsequent prompts **must** carry `[Authorize]`.

## ManagerDbContext Registration in WebAppExtensions

```csharp
builder.Services.AddDbContext<ManagerDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(ConfigurationKey.ConnectionString.ManagerDb),
        sql => sql.MigrationsAssembly(typeof(Program).Assembly.GetName().Name)
    )
);
```

## Acceptance Criteria

- `POST /api/v1/auth/token` with valid credentials returns `200` and a JWT.
- `POST /api/v1/auth/token` with wrong password returns `401`.
- Any future `[Authorize]` endpoint returns `401` without a token and `403` with an expired one.
- Database migration runs on startup and creates the `AdminUsers` table.
- Admin user is seeded from env vars on first run.
