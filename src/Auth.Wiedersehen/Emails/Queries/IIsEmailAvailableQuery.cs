namespace Auth.Wiedersehen.Emails.Queries;

public interface IIsEmailAvailableQuery
{
	Task<bool> ExecuteAsync(string email);
}
