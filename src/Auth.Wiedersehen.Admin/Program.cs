using Auth.Wiedersehen.Admin.Extensions;
using Serilog;

namespace Auth.Wiedersehen.Admin;

internal sealed class Program
{
	public static async Task Main(string[] args)
	{
		Log.Logger = new LoggerConfiguration()
			.WriteTo.Console()
			.CreateBootstrapLogger();
		Log.Information("Starting up");

		try
		{
			var builder = WebApplication.CreateBuilder(args);

			var app = builder
				.ConfigureLogging()
				.ConfigureServices()
				.Build();

			await app.ConfigurePipeline().RunAsync();
		}
		catch (Exception ex) when (ex.GetType().Name is not nameof(HostAbortedException))
		{
			Log.Fatal(ex, "Unhandled exception");
			throw;
		}
		finally
		{
			Log.Information("Shut down complete");
			await Log.CloseAndFlushAsync();
		}
	}
}
