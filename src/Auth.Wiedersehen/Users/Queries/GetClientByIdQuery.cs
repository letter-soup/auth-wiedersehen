using Auth.Wiedersehen.Extensions;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Wiedersehen.Users.Queries;

internal sealed class GetClientByIdQuery(
	ConfigurationDbContext configurationDbContext
) : IGetClientByIdQuery
{
	private readonly ConfigurationDbContext _configurationDbContext =
		configurationDbContext.Required(nameof(configurationDbContext));

	public async Task<Client?> ExecuteAsync(string clientId, CancellationToken ct)
	{
		return await _configurationDbContext.Clients
			.FirstOrDefaultAsync(c => c.ClientId == clientId, ct);
	}
}
