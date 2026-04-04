using Auth.Wiedersehen.Extensions;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Auth.Wiedersehen.Users.Queries;

internal sealed class GetClientRedirectUrisQuery(
	ConfigurationDbContext configurationDbContext
) : IGetClientRedirectUrisQuery
{
	private readonly ConfigurationDbContext _configurationDbContext =
		configurationDbContext.Required(nameof(configurationDbContext));

	public async Task<IReadOnlyList<string>?> ExecuteAsync(string clientId)
	{
		var client = await _configurationDbContext.Clients
			.Include(c => c.RedirectUris)
			.FirstOrDefaultAsync(c => c.ClientId == clientId);

		return client?.RedirectUris.Select(r => r.RedirectUri).ToList();
	}
}
