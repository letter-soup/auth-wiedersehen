using Auth.Wiedersehen.IntegrationTests.Extensions;
using Auth.Wiedersehen.IntegrationTests.Fixtures;

namespace Auth.Wiedersehen.IntegrationTests.Token;

public class AccessTokenIntegrationTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
	[Fact]
	public async Task AccessToken_GivenValidCredentials_ShouldSucceed()
	{
		// Arrange
		var user = await RegisterUserAsync();

		// Act
		var tokenResponse = await Client.RequestPasswordTokenAsync(
			TestClientId,
			TestClientSecret,
			user.Email,
			user.Password
		);

		// Assert
		tokenResponse.IsError.Should().BeFalse(tokenResponse.Error);
		tokenResponse.AccessToken.Should().NotBeNullOrWhiteSpace();
		tokenResponse.RefreshToken.Should().BeNullOrWhiteSpace();
	}

	[Fact]
	public async Task AccessToken_GivenWrongCredentials_ShouldFail()
	{
		// Arrange
		var user = await RegisterUserAsync();
		var wrongPassword = Fixture.CreatePassword();

		// Act
		var tokenResponse = await Client.RequestPasswordTokenAsync(
			TestClientId,
			TestClientSecret,
			user.Email,
			wrongPassword
		);

		// Assert
		tokenResponse.IsError.Should().BeTrue();
		tokenResponse.Error.Should().Be("invalid_grant");
	}
}
