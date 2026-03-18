using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Auth.Wiedersehen.Seeder.Dataset;

public class DevDataset : IEnvDataset
{
	public IEnumerable<IdentityResource> IdentityResources =>
	[
		new IdentityResources.OpenId(),
		new IdentityResources.Profile(),
		new IdentityResource
		{
			Name = "verification", UserClaims = new List<string> { JwtClaimTypes.Email, JwtClaimTypes.EmailVerified },
		},
	];

	public IEnumerable<ApiScope> ApiScopes => [new ApiScope("soup", "Soup API")];

	public IEnumerable<Client> Clients =>
	[
		new Client
		{
			ClientId = "soup-web",
			ClientSecrets = { new Secret("secret".Sha256()) },
			AllowedGrantTypes = GrantTypes.Code,
			RedirectUris = { "https://localhost:5000/login" },
			PostLogoutRedirectUris = { "https://localhost:5000/logoff" },
			AllowOfflineAccess = true,
			AllowedScopes =
			{
				IdentityServerConstants.StandardScopes.OpenId,
				IdentityServerConstants.StandardScopes.Profile,
				"verification",
				"soup",
			},
		},
	];
}
