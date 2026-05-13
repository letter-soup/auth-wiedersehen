# Prompt 00 — Shared Library: Auth.Wiedersehen.Shared

## Context

The existing `Auth.Wiedersehen` API lives at `src/Auth.Wiedersehen/` and contains several
cross-cutting utilities that the upcoming `Auth.Wiedersehen.ConfigurationManager.Api` also needs:

| File (current location) | What it provides |
|-------------------------|-----------------|
| `Exceptions/BaseApiException.cs` | Base exception class |
| `Exceptions/ValueRequiredException.cs` | Guard-clause exception |
| `Exceptions/HttpResponseException.cs` | Throwable exception → HTTP error response |
| `Exceptions/HttpResponseExceptionFilter.cs` | MVC action filter that converts the above to `ObjectResult` |
| `Exceptions/ErrorDetails.cs` | Shared error-response DTO |
| `Extensions/ObjectExtensions.cs` | `Required<T>` null-guard helper |
| `Extensions/ValidationResultExtensions.cs` | FluentValidation → `IEnumerable<KeyValuePair<string,string>>` |
| `Extensions/StringExtensions.cs` | `Normalize()` string helper |

`Extensions/IdentityResultExtensions.cs` is **not** shared — it depends on ASP.NET Identity
which the Configuration Manager API does not use.

## Goal

1. Create a new **class library** project `Auth.Wiedersehen.Shared` that houses the shared code.
2. Move (not copy) the files above from `Auth.Wiedersehen` into the library, updating namespaces.
3. Have `Auth.Wiedersehen` reference the new library and remove the now-redundant local files.
4. The upcoming `Auth.Wiedersehen.ConfigurationManager.Api` will also reference this library
   (that reference is added in Prompt 01).

## New Project

### `src/Auth.Wiedersehen.Shared/Auth.Wiedersehen.Shared.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <!-- Provides MVC types (IActionFilter, ObjectResult, StatusCodes, etc.) -->
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="12.1.1" />
  </ItemGroup>
</Project>
```

Add to `src/Auth.Wiedersehen.slnx`:
```xml
<Project Path="Auth.Wiedersehen.Shared/Auth.Wiedersehen.Shared.csproj" />
```

## Shared Source Files

All files use the root namespace `Auth.Wiedersehen.Shared`. All types must be `public`
(the originals were `internal` because everything was in one assembly).

### `src/Auth.Wiedersehen.Shared/Exceptions/BaseApiException.cs`

```csharp
namespace Auth.Wiedersehen.Shared.Exceptions;

public class BaseApiException : ApplicationException
{
    protected BaseApiException() { }
    public BaseApiException(string message) : base(message) { }
}
```

### `src/Auth.Wiedersehen.Shared/Exceptions/ValueRequiredException.cs`

```csharp
namespace Auth.Wiedersehen.Shared.Exceptions;

public class ValueRequiredException : BaseApiException
{
    private const string ErrorPrefix = "Missing required argument: {0}";

    public ValueRequiredException(string? paramName)
        : base(string.Format(ErrorPrefix, paramName)) { }

    public ValueRequiredException(string message, string? paramName)
        : base($"{string.Format(ErrorPrefix, paramName)}. {message}") { }
}
```

### `src/Auth.Wiedersehen.Shared/Exceptions/ErrorDetails.cs`

```csharp
namespace Auth.Wiedersehen.Shared.Exceptions;

public record ErrorDetails(int StatusCode, IEnumerable<KeyValuePair<string, string>>? Errors);
```

### `src/Auth.Wiedersehen.Shared/Exceptions/HttpResponseException.cs`

```csharp
namespace Auth.Wiedersehen.Shared.Exceptions;

public class HttpResponseException(
    IEnumerable<KeyValuePair<string, string>> errors,
    int statusCode = StatusCodes.Status400BadRequest
) : BaseApiException
{
    public int StatusCode { get; } = statusCode;
    public IEnumerable<KeyValuePair<string, string>>? Errors { get; } = errors;
}
```

### `src/Auth.Wiedersehen.Shared/Exceptions/HttpResponseExceptionFilter.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Auth.Wiedersehen.Shared.Exceptions;

public class HttpResponseExceptionFilter : IOrderedFilter, IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is not HttpResponseException exception) return;

        context.Result = new ObjectResult(new ErrorDetails(exception.StatusCode, exception.Errors))
        {
            StatusCode = exception.StatusCode,
        };
        context.ExceptionHandled = true;
    }

    public int Order => int.MaxValue - 10;
}
```

### `src/Auth.Wiedersehen.Shared/Extensions/ObjectExtensions.cs`

```csharp
using Auth.Wiedersehen.Shared.Exceptions;

namespace Auth.Wiedersehen.Shared.Extensions;

public static class ObjectExtensions
{
    public static T Required<T>(this T? argument, string? paramName)
        => argument ?? throw new ValueRequiredException(paramName);
}
```

### `src/Auth.Wiedersehen.Shared/Extensions/ValidationResultExtensions.cs`

```csharp
using FluentValidation.Results;

namespace Auth.Wiedersehen.Shared.Extensions;

public static class ValidationResultExtensions
{
    public static IEnumerable<KeyValuePair<string, string>> ToKeyValuePairs(this ValidationResult result)
        => result.Errors.Select(f => new KeyValuePair<string, string>(f.PropertyName, f.ErrorMessage));
}
```

### `src/Auth.Wiedersehen.Shared/Extensions/StringExtensions.cs`

```csharp
namespace Auth.Wiedersehen.Shared.Extensions;

public static class StringExtensions
{
    public static string Normalize(this string str) => str.ToLower();
}
```

## Changes to `Auth.Wiedersehen`

### 1. Add project reference in `Auth.Wiedersehen.csproj`

```xml
<ItemGroup>
  <ProjectReference Include="..\Auth.Wiedersehen.Shared\Auth.Wiedersehen.Shared.csproj" />
</ItemGroup>
```

### 2. Delete the now-moved files

Remove from `src/Auth.Wiedersehen/`:
- `Exceptions/BaseApiException.cs`
- `Exceptions/ValueRequiredException.cs`
- `Exceptions/ErrorDetails.cs`
- `Exceptions/HttpResponseException.cs`
- `Exceptions/HttpResponseExceptionFilter.cs`
- `Extensions/ObjectExtensions.cs`
- `Extensions/ValidationResultExtensions.cs`
- `Extensions/StringExtensions.cs`

Keep: `Extensions/IdentityResultExtensions.cs`, `Extensions/WebAppExtensions.cs`.

### 3. Update `using` statements across `Auth.Wiedersehen`

Every file in `Auth.Wiedersehen` that previously used:
```csharp
using Auth.Wiedersehen.Exceptions;
using Auth.Wiedersehen.Extensions;
```
must add (or replace with):
```csharp
using Auth.Wiedersehen.Shared.Exceptions;
using Auth.Wiedersehen.Shared.Extensions;
```

Files known to be affected:
- `Extensions/WebAppExtensions.cs` (registers `HttpResponseExceptionFilter`)
- `Users/UserController.cs`
- `Users/Commands/CreateUserCommand.cs`
- `Users/UserModels.cs` (validator)
- `Emails/EmailController.cs`
- Any other file referencing `HttpResponseException`, `Required`, or `ToKeyValuePairs`

### 4. `Auth.Wiedersehen.IntegrationTests` / `Auth.Wiedersehen.UnitTests`

If either test project references `ErrorDetails` or other moved types, add the same project
reference and update namespaces.

## Acceptance Criteria

- `dotnet build src/Auth.Wiedersehen.slnx` succeeds with no namespace or type-not-found errors.
- `Auth.Wiedersehen.Shared` contains no references to ASP.NET Identity or IdentityServer.
- All shared types are `public` in the library.
- `Auth.Wiedersehen` has no duplicate copies of the moved files.
