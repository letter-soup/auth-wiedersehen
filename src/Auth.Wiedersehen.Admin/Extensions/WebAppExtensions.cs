using Auth.Wiedersehen.Admin.Configuration;
using Auth.Wiedersehen.Shared.Exceptions;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Auth.Wiedersehen.Admin.Extensions;

internal static class WebAppExtensions
{
	public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
	{
		builder.Host.UseSerilog(
			(ctx, lc) => lc
				.WriteTo.Console(
					outputTemplate:
					"[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}"
				)
				.Enrich.FromLogContext()
				.ReadFrom.Configuration(ctx.Configuration),
			true
		);

		return builder;
	}

	public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
	{
		builder.AddConfiguration();

		builder.Services.AddControllers(options => options.Filters.Add<HttpResponseExceptionFilter>());
		builder.Services.AddOpenApi();

		builder.Services.AddSingleton(new ConfigurationStoreOptions());
		builder.Services.AddDbContext<ConfigurationDbContext>(options =>
			options.UseNpgsql(
				builder.Configuration.GetConnectionString(ConfigurationKey.ConnectionString.ConfigurationDb)
			)
		);

		// TODO: Prompt 02 — configure TokenValidationParameters
		builder.Services
			.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer();

		builder.Services.AddAuthorization();

		return builder;
	}

	public static WebApplication ConfigurePipeline(this WebApplication app)
	{
		app.UseSerilogRequestLogging();
		app.UseAuthentication();
		app.UseAuthorization();
		app.MapControllers();
		app.MapOpenApi();

		if (app.Environment.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}

		return app;
	}
}
