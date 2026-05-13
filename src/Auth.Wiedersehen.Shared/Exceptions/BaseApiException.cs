namespace Auth.Wiedersehen.Shared.Exceptions;

public class BaseApiException : ApplicationException
{
    protected BaseApiException() { }
    public BaseApiException(string message) : base(message) { }
}
