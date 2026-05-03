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
			Constants.TestClientId,
			Constants.TestClientSecret,
			user.Email,
			user.Password
		);
		tokenResponse.IsError.Should().BeFalse(tokenResponse.Error);

		// Act
		var revocationResponse = await Client.RevokeTokenAsync(
			Constants.TestClientId,
			Constants.TestClientSecret,
			tokenResponse.AccessToken!
		);

		// Assert
		revocationResponse.IsError.Should().BeFalse(revocationResponse.Error);
	}
}
