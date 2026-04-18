using Auth.Wiedersehen.Configuration;
using Auth.Wiedersehen.Database.Migrations;
using Auth.Wiedersehen.Seeder.Dataset;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Auth.Wiedersehen.IntegrationTests.Fixtures;

[UsedImplicitly]
public class IntegrationTestFixture : IAsyncLifetime
{
	private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:17").Build();

	internal WebApplicationFactory<Program> Factory { get; private set; } = null!;

	public async ValueTask InitializeAsync()
	{
		// 1. Start the database container
		await _dbContainer.StartAsync();
		var connectionString = _dbContainer.GetConnectionString();

		// 2. Create and configure the shared WebApplicationFactory
		Factory = new WebApplicationFactory<Program>()
			.WithWebHostBuilder(builder =>
				{
					builder.UseEnvironment("Tests");
					builder.ConfigureAppConfiguration((_, config) =>
						{
							config.AddInMemoryCollection(
								new Dictionary<string, string?>
								{
									{
										$"ConnectionStrings:{ConfigurationKey.ConnectionString.ApplicationDb}",
										connectionString
									},
									{
										$"ConnectionStrings:{ConfigurationKey.ConnectionString.ConfigurationDb}",
										connectionString
									},
									{
										$"ConnectionStrings:{ConfigurationKey.ConnectionString.PersistentGrandDb}",
										connectionString
									},
								}
							);
						}
					);
				}
			);

		// 3. Run migrations once
		using var scope = Factory.Services.CreateScope();
		await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
		await scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>().Database.MigrateAsync();
		await scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.MigrateAsync();

		// 4. Seed data once
		SeedIdentityServer(scope.ServiceProvider);
	}

	public async ValueTask DisposeAsync()
	{
		await Factory.DisposeAsync();
		await _dbContainer.StopAsync();
	}

	private void SeedIdentityServer(IServiceProvider serviceProvider)
	{
		var context = serviceProvider.GetRequiredService<ConfigurationDbContext>();
		var dataset = new DevDataset();

		if (!context.Clients.Any())
		{
			foreach (var client in dataset.Clients)
			{
				context.Clients.Add(client.ToEntity());
			}

			// Add a test client for integration tests
			context.Clients.Add(
				new Client
				{
					ClientId = Constants.TestClientId,
					ClientSecrets = { new Secret(Constants.TestClientSecret.Sha256()) },
					AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
					AllowedScopes =
					{
						"openid",
						"profile",
						"soup",
					},
					AllowOfflineAccess = true,
					RedirectUris = [Constants.TestClientRedirectUri]
				}.ToEntity()
			);

			context.SaveChanges();
		}

		if (!context.IdentityResources.Any())
		{
			foreach (var resource in dataset.IdentityResources)
			{
				context.IdentityResources.Add(resource.ToEntity());
			}

			context.SaveChanges();
		}

		if (!context.ApiScopes.Any())
		{
			foreach (var scope in dataset.ApiScopes)
			{
				context.ApiScopes.Add(scope.ToEntity());
			}

			context.SaveChanges();
		}
	}
}
