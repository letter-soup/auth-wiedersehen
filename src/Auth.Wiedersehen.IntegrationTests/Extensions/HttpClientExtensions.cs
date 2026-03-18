using System.Net.Http.Json;
using Auth.Wiedersehen.Users;
using Duende.IdentityModel.Client;

namespace Auth.Wiedersehen.IntegrationTests.Extensions;

public static class HttpClientExtensions
{
	private static HttpResponseMessage VerifyResponse(HttpResponseMessage response, HttpClientMode clientCallMode)
	{
		switch (clientCallMode)
		{
			case HttpClientMode.VerifySuccess:
				response.IsSuccessStatusCode.Should().BeTrue();
				break;
			case HttpClientMode.VerifyFailure:
				response.IsSuccessStatusCode.Should().BeFalse();
				break;
			case HttpClientMode.SkipVerification:
			default:
				break;
		}

		return response;
	}

	extension(HttpClient client)
	{
		public async Task<HttpResponseMessage> CreateUserAsync(
			CreateUserRequest request,
			HttpClientMode mode = HttpClientMode.SkipVerification
		)
		{
			return VerifyResponse(await client.PostAsJsonAsync("/api/v1/user", request), mode);
		}

		public async Task<HttpResponseMessage> IsEmailAvailableAsync(
			string email,
			HttpClientMode mode = HttpClientMode.SkipVerification
		)
		{
			return VerifyResponse(await client.GetAsync($"/api/v1/email/is-available?email={email}"), mode);
		}

		public async Task<TokenResponse> RequestPasswordTokenAsync(
			string clientId,
			string clientSecret,
			string email,
			string password,
			string scope = "openid profile soup"
		)
		{
			var discovery = await client.GetDiscoveryDocumentAsync();
			discovery.IsError.Should().BeFalse(discovery.Error);

			return await client.RequestPasswordTokenAsync(
				new PasswordTokenRequest
				{
					Address = discovery.TokenEndpoint,
					ClientId = clientId,
					ClientSecret = clientSecret,
					UserName = email,
					Password = password,
					Scope = scope,
				}
			);
		}

		public async Task<TokenResponse> RequestRefreshTokenAsync(
			string clientId,
			string clientSecret,
			string refreshToken
		)
		{
			var discovery = await client.GetDiscoveryDocumentAsync();
			discovery.IsError.Should().BeFalse(discovery.Error);

			return await client.RequestRefreshTokenAsync(
				new RefreshTokenRequest
				{
					Address = discovery.TokenEndpoint,
					ClientId = clientId,
					ClientSecret = clientSecret,
					RefreshToken = refreshToken,
				}
			);
		}

		public async Task<TokenRevocationResponse> RevokeTokenAsync(
			string clientId,
			string clientSecret,
			string token,
			string tokenTypeHint = "access_token"
		)
		{
			var discovery = await client.GetDiscoveryDocumentAsync();
			discovery.IsError.Should().BeFalse(discovery.Error);

			return await client.RevokeTokenAsync(
				new TokenRevocationRequest
				{
					Address = discovery.RevocationEndpoint,
					ClientId = clientId,
					ClientSecret = clientSecret,
					Token = token,
					TokenTypeHint = tokenTypeHint,
				}
			);
		}
	}

	extension(HttpResponseMessage message)
	{
		public async Task<T?> As<T>()
		{
			return await message.Content.ReadFromJsonAsync<T>();
		}
	}
}
