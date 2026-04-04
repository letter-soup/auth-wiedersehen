using Duende.IdentityServer.EntityFramework.Entities;

namespace Auth.Wiedersehen.Users.Queries;

public interface IGetClientByIdQuery
{
	Task<Client?> ExecuteAsync(string clientId, CancellationToken ct = default);
}
