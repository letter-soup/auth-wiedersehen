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

	public async Task<IReadOnlyList<string>> ExecuteAsync(string clientId, CancellationToken ct)
	{
		var client = await _configurationDbContext.Clients
			.Include(c => c.RedirectUris)
			.FirstOrDefaultAsync(c => c.ClientId == clientId, ct);

		return client is null
			? Array.Empty<string>()
			: client.RedirectUris.Select(r => r.RedirectUri).ToList();
	}
}
