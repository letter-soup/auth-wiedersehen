using Auth.Wiedersehen.IntegrationTests.Extensions;
using Auth.Wiedersehen.IntegrationTests.Fixtures;

namespace Auth.Wiedersehen.IntegrationTests.Token;

public class RevokeTokenIntegrationTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
	[Fact]
	public async Task RevokeToken_GivenValidToken_ShouldSucceed()
	{
		// Arrange
		var user = await RegisterUserAsync();
		var tokenResponse = await Client.RequestPasswordTokenAsync(
			TestClientId,
			TestClientSecret,
			user.Email,
			user.Password
		);
		tokenResponse.IsError.Should().BeFalse(tokenResponse.Error);

		// Act
		var revocationResponse = await Client.RevokeTokenAsync(
			TestClientId,
			TestClientSecret,
			tokenResponse.AccessToken!
		);

		// Assert
		revocationResponse.IsError.Should().BeFalse(revocationResponse.Error);
	}
}
