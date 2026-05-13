using Auth.Wiedersehen.Shared.Exceptions;

namespace Auth.Wiedersehen.Shared.Extensions;

public static class ObjectExtensions
{
    public static T Required<T>(this T? argument, string? paramName)
        => argument ?? throw new ValueRequiredException(paramName);
}
