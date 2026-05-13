namespace Auth.Wiedersehen.Admin.Configuration;

internal struct ConfigurationKey
{
	public struct ConnectionString
	{
		public const string ConfigurationDb = "ConfigurationDB";
		public const string ManagerDb       = "ManagerDB";
	}

	public struct Jwt
	{
		public const string SigningKey     = "Jwt:SigningKey";
		public const string Issuer        = "Jwt:Issuer";
		public const string Audience      = "Jwt:Audience";
		public const string ExpiryMinutes = "Jwt:ExpiryMinutes";
	}

	public struct Admin
	{
		public const string Username = "Admin:Username";
		public const string Password = "Admin:Password";
	}
}
