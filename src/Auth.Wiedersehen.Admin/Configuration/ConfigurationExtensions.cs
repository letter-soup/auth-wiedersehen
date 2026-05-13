namespace Auth.Wiedersehen.Admin.Configuration;

internal static class ConfigurationExtensions
{
	private const string EnvVarPrefix = "AWA_";

	extension(IHostApplicationBuilder builder)
	{
		public IHostApplicationBuilder AddConfiguration()
		{
			builder.Configuration
				.AddJsonFile(builder.GetAppSettingPath())
				.AddEnvironmentVariables(EnvVarPrefix);

			return builder;
		}

		private string GetAppSettingPath()
		{
			return $"AppSettings/appsettings.{builder.GetEnvironmentName()}.json";
		}

		private string GetEnvironmentName()
		{
			return string.IsNullOrWhiteSpace(builder.Environment.EnvironmentName)
				? "Production"
				: builder.Environment.EnvironmentName;
		}
	}
}
