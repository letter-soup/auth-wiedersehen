namespace Auth.Wiedersehen.Shared.Exceptions;

public record ErrorDetails(int StatusCode, IEnumerable<KeyValuePair<string, string>>? Errors);
