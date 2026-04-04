namespace Auth.Wiedersehen.Users;

public interface IRedirectUriValidator
{
	Task<string?> ValidateAsync(string? clientId, string? redirectUri);
}
