namespace Auth.Wiedersehen.Localization;

internal static class LocalizationExtensions
{
	extension(IHostApplicationBuilder builder)
	{
		public IHostApplicationBuilder AddLocalization()
		{
			builder.Services.AddTransient<ILocalizer, LocalizerAdapter>();
			builder.Services.AddLocalization();

			return builder;
		}
	}

	extension(IApplicationBuilder app)
	{
		public IApplicationBuilder UseLocalization()
		{
			var supportedCultures = new[] { "en-US", "en" };
			var localizationOptions = new RequestLocalizationOptions()
				.SetDefaultCulture(supportedCultures[0])
				.AddSupportedCultures(supportedCultures)
				.AddSupportedUICultures(supportedCultures);

			app.UseRequestLocalization(localizationOptions);

			return app;
		}
	}
}
