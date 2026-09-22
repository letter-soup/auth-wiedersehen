using FluentValidation.Results;

namespace Auth.Wiedersehen.Shared.Extensions;

public static class ValidationResultExtensions
{
    public static IEnumerable<KeyValuePair<string, string>> ToKeyValuePairs(this ValidationResult result)
        => result.Errors.Select(f => new KeyValuePair<string, string>(f.PropertyName, f.ErrorMessage));
}
