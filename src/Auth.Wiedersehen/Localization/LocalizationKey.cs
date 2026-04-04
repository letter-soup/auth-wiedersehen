namespace Auth.Wiedersehen.Localization;

internal struct LocalizationKey
{
	public struct Error
	{
		public struct Email
		{
			public const string Invalid = "Error:Email:Invalid";
			public const string Missing = "Error:Email:Missing";
		}

		public struct Password
		{
			public const string DigitMissing = "Error:Password:DigitMissing";
			public const string LowercaseMissing = "Error:Password:LowercaseMissing";
			public const string Missing = "Error:Password:Missing";
			public const string SpecialMissing = "Error:Password:SpecialMissing";
			public const string TooShort = "Error:Password:TooShort";
			public const string UppercaseMissing = "Error:Password:UppercaseMissing";
		}

		public struct Terms
		{
			public const string NotAccepted = "Error:Terms:NotAccepted";
		}

		public struct Client
		{
			public const string NotFound = "Error:Client:NotFound";
			public const string Missing = "Error:Client:Missing";
		}

		public struct Redirect
		{
			public const string Invalid = "Error:Redirect:Invalid";
			public const string Missing = "Error:Redirect:Missing";
			public const string NotFound = "Error:Redirect:NotFound";
		}
	}
}
