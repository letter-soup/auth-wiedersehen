namespace Auth.Wiedersehen.Users.Queries;

public interface IGetClientRedirectUrisQuery
{
	Task<IReadOnlyList<string>?> ExecuteAsync(string clientId);
}
