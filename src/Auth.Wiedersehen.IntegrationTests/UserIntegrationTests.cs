using System.Net;
using Auth.Wiedersehen.IntegrationTests.Extensions;
using Auth.Wiedersehen.IntegrationTests.Fixtures;
using Auth.Wiedersehen.Users;

namespace Auth.Wiedersehen.IntegrationTests;

public class UserIntegrationTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
	private CreateUserRequest ValidRequest => new CreateUserRequest(
		Fixture.CreateEmail(),
		Fixture.CreatePassword(),
		true,
		Constants.TestClientId,
		Constants.TestClientRedirectUri
	);

	[Fact]
	public async Task CreateUser_GivenValidData_Returns201Created()
	{
		// Act
		var response = await Client.CreateUserAsync(ValidRequest);

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
		var request = await RegisterUserAsync();

		// Act
		var response = await Client.CreateUserAsync(request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Conflict);
	}

	[Fact]
	public async Task CreateUser_GivenWeakPassword_Returns400BadRequest()
	{
		// Act
		var response = await Client.CreateUserAsync(
			ValidRequest with { Password = Fixture.CreatePassword(config: PasswordConfig.LowerCase) }
		);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task CreateUser_GivenTermsNotAccepted_Returns400BadRequest()
	{
		// Act
		var response = await Client.CreateUserAsync(ValidRequest with { TermsAccepted = false });

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}
}
