using Auth.Wiedersehen.Authentication;
using Auth.Wiedersehen.Configuration;
using Auth.Wiedersehen.Database.Migrations;
using Auth.Wiedersehen.Emails;
using Auth.Wiedersehen.Exceptions;
using Auth.Wiedersehen.Localization;
using Auth.Wiedersehen.Users;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Auth.Wiedersehen.Extensions;

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
		builder.Services.AddScoped<IUserService, UserService>();
		builder.Services.AddScoped<IEmailService, EmailService>();
		builder.Services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
		builder.Services.AddScoped<IValidator<EmailAvailableRequest>, EmailAvailableRequestValidator>();

		builder.AddLocalization();
		builder.AddConfiguration();

		builder.Services.AddControllers(options => options.Filters.Add<HttpResponseExceptionFilter>());
		builder.Services.AddOpenApi();

		builder.Services.AddDbContext<ApplicationDbContext>(options =>
			options.UseNpgsql(
				builder.Configuration.GetConnectionString(ConfigurationKey.ConnectionString.ApplicationDb)
			)
		);

		builder.AddAuthentication();

		return builder;
	}

	public static WebApplication ConfigurePipeline(this WebApplication app)
	{
		app.UseLocalization();

		app.UseSerilogRequestLogging();
		app.MapControllers();
		app.MapOpenApi();

		if (app.Environment.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}

		app.UseAuthentication();

		return app;
	}
}
