using System.Text;

namespace Auth.Wiedersehen.UnitTests.Extensions;

public static class FixtureExtensions
{
	private const string Domain = "example.com";

	public static string CreateEmail(this IFixture fixture)
	{
		return $"{fixture.Create<string>()}@{Domain}";
	}

	public static string CreatePassword(
		this IFixture _,
		int length = 8,
		PasswordConfig config = PasswordConfig.All
	)
	{
		var lowerCaseAlphabet = "abcdefghijklmnopqrstuvwxyz";
		var upperCaseAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		var digitAlphabet = "0123456789";
		var specialAlphabet = "!@#$%_+";
		var rnd = new Random();

		var alphabetBuilder = new StringBuilder();
		var passwordBuilder = new StringBuilder();

		if (config.HasFlag(PasswordConfig.LowerCase))
		{
			alphabetBuilder.Append(lowerCaseAlphabet);
			passwordBuilder.Append(lowerCaseAlphabet[rnd.Next(0, lowerCaseAlphabet.Length)]);
		}

		if (config.HasFlag(PasswordConfig.UpperCase))
		{
			alphabetBuilder.Append(upperCaseAlphabet);
			passwordBuilder.Append(upperCaseAlphabet[rnd.Next(0, upperCaseAlphabet.Length)]);
		}

		if (config.HasFlag(PasswordConfig.Digits))
		{
			alphabetBuilder.Append(digitAlphabet);
			passwordBuilder.Append(digitAlphabet[rnd.Next(0, digitAlphabet.Length)]);
		}

		if (config.HasFlag(PasswordConfig.Special))
		{
			alphabetBuilder.Append(specialAlphabet);
			passwordBuilder.Append(specialAlphabet[rnd.Next(0, specialAlphabet.Length)]);
		}

		var alphabet = alphabetBuilder.ToString();
		while (passwordBuilder.Length < length)
		{
			passwordBuilder.Append(alphabet[rnd.Next(0, alphabet.Length)]);
		}

		return passwordBuilder.ToString();
	}

	public static string CreateUri(this IFixture fixture)
	{
		return new Uri($"https://{fixture.Create<string>()}.com").AbsoluteUri;
	}
}
