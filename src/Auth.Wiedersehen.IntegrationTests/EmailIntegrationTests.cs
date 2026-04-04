using System.Net;
using Auth.Wiedersehen.IntegrationTests.Extensions;
using Auth.Wiedersehen.IntegrationTests.Fixtures;
using Auth.Wiedersehen.Users;

namespace Auth.Wiedersehen.IntegrationTests;

public class EmailIntegrationTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
	[Fact]
	public async Task IsAvailable_GivenNewEmail_Returns200OK()
	{
		// Arrange
		var email = Fixture.CreateEmail();

		// Act
		var response = await Client.IsEmailAvailableAsync(email);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task IsAvailable_GivenExistingEmail_Returns409Conflict()
	{
		// Arrange
		var email = Fixture.CreateEmail();
		var request = new CreateUserRequest(email, Fixture.CreatePassword(), true, Fixture.Create<string>(), Fixture.CreateUri());
		await Client.CreateUserAsync(request, HttpClientMode.VerifySuccess);

		// Act
		var response = await Client.IsEmailAvailableAsync(email);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Conflict);
	}

	[Fact]
	public async Task IsAvailable_GivenInvalidEmail_Returns400BadRequest()
	{
		// Arrange
		var email = Fixture.Create<string>();

		// Act
		var response = await Client.IsEmailAvailableAsync(email);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}
}
