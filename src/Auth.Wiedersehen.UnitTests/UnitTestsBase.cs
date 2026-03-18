using Auth.Wiedersehen.Configuration;
using Auth.Wiedersehen.Localization;
using Microsoft.Extensions.Configuration;

namespace Auth.Wiedersehen.UnitTests;

public class UnitTestsBase
{
	private const int PasswordMinLength = 8;
	protected readonly IConfiguration Configuration;
	protected readonly IFixture Fixture;
	protected readonly ILocalizer Localizer;

	protected UnitTestsBase()
	{
		var configMock = new Mock<IConfiguration>();

		var passwordMinLength = new Mock<IConfigurationSection>();
		passwordMinLength.Setup(x => x.Value).Returns(PasswordMinLength.ToString());
		configMock.Setup(x => x.GetSection(ConfigurationKey.Password.MinLength)).Returns(passwordMinLength.Object);

		Configuration = configMock.Object;
		Fixture = new Fixture();
		Localizer = SetupLocalizer();
	}

	private ILocalizer SetupLocalizer()
	{
		var localizerMock = new Mock<ILocalizer>();
		localizerMock.Setup(x => x[It.IsAny<string>()]).Returns((string key) => key);
		localizerMock.Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()]).Returns((string key, object[] _) => key);
		return localizerMock.Object;
	}
}
