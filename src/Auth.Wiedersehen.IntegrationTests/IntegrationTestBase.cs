using Auth.Wiedersehen.IntegrationTests.Extensions;
using Auth.Wiedersehen.IntegrationTests.Fixtures;
using Auth.Wiedersehen.Users;

namespace Auth.Wiedersehen.IntegrationTests;

[Collection(nameof(IntegrationTestsCollection))]
public abstract class IntegrationTestBase(IntegrationTestFixture fixture) : IAsyncLifetime
{
	protected readonly IFixture Fixture = new Fixture();
	// private IServiceScope _transactionScope = null!;

	protected HttpClient Client { get; private set; } = null!;

	public ValueTask InitializeAsync()
	{
		try
		{
			Client = fixture.Factory.CreateClient();
			return ValueTask.CompletedTask;
		}
		catch (Exception exception)
		{
			return ValueTask.FromException(exception);
		}
	}

	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);
		return ValueTask.CompletedTask;
	}

	protected async Task<CreateUserRequest> RegisterUserAsync()
	{
		var request = new CreateUserRequest(
			Fixture.CreateEmail(),
			Fixture.CreatePassword(),
			true,
			Constants.TestClientId,
			Constants.TestClientRedirectUri
		);
		await Client.CreateUserAsync(request, HttpClientMode.VerifySuccess);
		return request;
	}
}
