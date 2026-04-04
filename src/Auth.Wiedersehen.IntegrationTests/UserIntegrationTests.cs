using System.Net;
using Auth.Wiedersehen.IntegrationTests.Extensions;
using Auth.Wiedersehen.IntegrationTests.Fixtures;
using Auth.Wiedersehen.Users;

namespace Auth.Wiedersehen.IntegrationTests;

public class UserIntegrationTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
	[Fact]
	public async Task CreateUser_GivenValidData_Returns201Created()
	{
		// Arrange
		var request = new CreateUserRequest(Fixture.CreateEmail(), Fixture.CreatePassword(), true, Fixture.Create<string>(), Fixture.CreateUri());

		// Act
		var response = await Client.CreateUserAsync(request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Created);

		var content = await response.As<CreateUserResponse>();
		content.Should().NotBeNull();
		content.UserId.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task CreateUser_GivenDuplicateEmail_Returns409Conflict()
	{
		// Arrange
		var request = new CreateUserRequest(Fixture.CreateEmail(), Fixture.CreatePassword(), true, Fixture.Create<string>(), Fixture.CreateUri());
		await Client.CreateUserAsync(request, HttpClientMode.VerifySuccess);

		// Act
		var response = await Client.CreateUserAsync(request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Conflict);
	}

	[Fact]
	public async Task CreateUser_GivenWeakPassword_Returns400BadRequest()
	{
		// Arrange
		var request = new CreateUserRequest(
			Fixture.CreateEmail(),
			Fixture.CreatePassword(config: PasswordConfig.LowerCase),
			true, Fixture.Create<string>(), Fixture.CreateUri()
		);

		// Act
		var response = await Client.CreateUserAsync(request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task CreateUser_GivenTermsNotAccepted_Returns400BadRequest()
	{
		// Arrange
		var request = new CreateUserRequest(Fixture.CreateEmail(), Fixture.CreatePassword(), false, Fixture.Create<string>(), Fixture.CreateUri());

		// Act
		var response = await Client.CreateUserAsync(request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}
}
