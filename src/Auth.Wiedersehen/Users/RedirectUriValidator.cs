using Auth.Wiedersehen.Exceptions;
using Auth.Wiedersehen.Extensions;
using Auth.Wiedersehen.Localization;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Auth.Wiedersehen.Users;

internal sealed class RedirectUriValidator(
	ConfigurationDbContext configurationDbContext,
	ILocalizer localizer
) : IRedirectUriValidator
{
	private readonly ConfigurationDbContext _configurationDbContext =
		configurationDbContext.Required(nameof(configurationDbContext));

	private readonly ILocalizer _localizer = localizer.Required(nameof(localizer));

	public async Task<string?> ValidateAsync(string? clientId, string? redirectUri)
	{
		if (string.IsNullOrWhiteSpace(clientId) && string.IsNullOrWhiteSpace(redirectUri))
		{
			return null;
		}

		if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(redirectUri))
		{
			throw new HttpResponseException(
				[new KeyValuePair<string, string>("RedirectUri", _localizer[LocalizationKey.Error.Redirect.InvalidRedirectUri])]
			);
		}

		var client = await _configurationDbContext.Clients
			.Include(c => c.RedirectUris)
			.FirstOrDefaultAsync(c => c.ClientId == clientId);

		if (client is null)
		{
			throw new HttpResponseException(
				[new KeyValuePair<string, string>("ClientId", _localizer[LocalizationKey.Error.Redirect.InvalidClient])]
			);
		}

		var isAllowed = client.RedirectUris.Any(r => r.RedirectUri == redirectUri);

		if (!isAllowed)
		{
			throw new HttpResponseException(
				[new KeyValuePair<string, string>("RedirectUri", _localizer[LocalizationKey.Error.Redirect.InvalidRedirectUri])]
			);
		}

		return redirectUri;
	}
}
