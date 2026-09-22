namespace Auth.Wiedersehen.Shared.Exceptions;

public class ValueRequiredException : BaseApiException
{
    private const string ErrorPrefix = "Missing required argument: {0}";

    public ValueRequiredException(string? paramName)
        : base(string.Format(ErrorPrefix, paramName)) { }

    public ValueRequiredException(string message, string? paramName)
        : base($"{string.Format(ErrorPrefix, paramName)}. {message}") { }
}
