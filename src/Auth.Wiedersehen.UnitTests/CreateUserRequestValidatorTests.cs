using Auth.Wiedersehen.Configuration;
using Auth.Wiedersehen.Localization;
using Auth.Wiedersehen.UnitTests.Extensions;
using Auth.Wiedersehen.Users;
using Auth.Wiedersehen.Users.Queries;
using Duende.IdentityServer.EntityFramework.Entities;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Configuration;

namespace Auth.Wiedersehen.UnitTests;

public class CreateUserRequestValidatorTests : UnitTestsBase
{
	private readonly CreateUserRequestValidator _validator;

	private readonly string _validClientId;
	private readonly string _validRedirectUri;
	private readonly string _invalidClientId;

	public CreateUserRequestValidatorTests()
	{
		_validClientId = Fixture.Create<string>();
		_validRedirectUri = Fixture.CreateUri();
		_invalidClientId = Fixture.Create<string>();

		var clientByIdQueryMock = new Mock<IGetClientByIdQuery>();
		var redirectUrisQueryMock = new Mock<IGetClientRedirectUrisQuery>();

		clientByIdQueryMock
			.Setup(x => x.ExecuteAsync(It.Is<string>(y => y == _validClientId), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new Client { ClientId = _validClientId });
		clientByIdQueryMock
			.Setup(x => x.ExecuteAsync(It.Is<string>(y => y == _invalidClientId), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => null);
		redirectUrisQueryMock
			.Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => [Fixture.CreateUri()]);
		redirectUrisQueryMock
			.Setup(x => x.ExecuteAsync(It.Is<string>(y => y == _validClientId), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => [_validRedirectUri]);

		_validator = new CreateUserRequestValidator(
			Configuration,
			Localizer,
			clientByIdQueryMock.Object,
			redirectUrisQueryMock.Object
		);
	}

	private CreateUserRequest ValidRequest =>
		new CreateUserRequest(
			Fixture.CreateEmail(),
			Fixture.CreatePassword(),
			true,
			_validClientId,
			_validRedirectUri
		);

	[Fact]
	public async Task GivenValidRequest_ShouldHaveNoError()
	{
		var result = await _validator.TestValidateAsync(
			ValidRequest,
			cancellationToken: TestContext.Current.CancellationToken
		);
		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public async Task GivenEmptyEmail_ShouldHaveError()
	{
		var result = await _validator.TestValidateAsync(
			ValidRequest with { Email = string.Empty },
			cancellationToken: TestContext.Current.CancellationToken
		);
		result.ShouldHaveValidationErrorFor(x => x.Email);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Email.Missing);
	}

	[Fact]
	public async Task GivenInvalidEmail_ShouldHaveError()
	{
		var result = await _validator.TestValidateAsync(
			ValidRequest with { Email = Fixture.Create<string>() },
			cancellationToken: TestContext.Current.CancellationToken
		);
		result.ShouldHaveValidationErrorFor(x => x.Email);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Email.Invalid);
	}

	[Fact]
	public async Task GivenEmptyPassword_ShouldHaveError()
	{
		var result = await _validator.TestValidateAsync(
			ValidRequest with { Password = string.Empty },
			cancellationToken: TestContext.Current.CancellationToken
		);
		result.ShouldHaveValidationErrorFor(x => x.Password);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Password.Missing);
	}

	[Fact]
	public async Task GivenShortPassword_ShouldHaveError()
	{
		var passwordLength = Configuration.GetValue<int>(ConfigurationKey.Password.MinLength) - 1;
		var result = await _validator.TestValidateAsync(
			ValidRequest with { Password = Fixture.CreatePassword(passwordLength) },
			cancellationToken: TestContext.Current.CancellationToken
		);
		result.ShouldHaveValidationErrorFor(x => x.Password);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Password.TooShort);
	}

	[Fact]
	public async Task GivenPasswordWithoutUppercase_ShouldHaveError()
	{
		var result = await _validator.TestValidateAsync(
			ValidRequest with
			{
				Password = Fixture.CreatePassword(config: PasswordConfig.All & ~PasswordConfig.UpperCase)
			},
			cancellationToken: TestContext.Current.CancellationToken
		);
		result.ShouldHaveValidationErrorFor(x => x.Password);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Password.UppercaseMissing);
	}

	[Fact]
	public async Task GivenPasswordWithoutLowercase_ShouldHaveError()
	{
		var result = await _validator.TestValidateAsync(
			ValidRequest with
			{
				Password = Fixture.CreatePassword(config: PasswordConfig.All & ~PasswordConfig.LowerCase)
			},
			cancellationToken: TestContext.Current.CancellationToken
		);
		result.ShouldHaveValidationErrorFor(x => x.Password);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Password.LowercaseMissing);
	}

	[Fact]
	public async Task GivenPasswordWithoutDigit_ShouldHaveError()
	{
		var result = await _validator.TestValidateAsync(
			ValidRequest with
			{
				Password = Fixture.CreatePassword(config: PasswordConfig.All & ~PasswordConfig.Digits)
			},
			cancellationToken: TestContext.Current.CancellationToken
		);
		result.ShouldHaveValidationErrorFor(x => x.Password);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Password.DigitMissing);
	}

	[Fact]
	public async Task GivenPasswordWithoutSpecialCharacter_ShouldHaveError()
	{
		var result = await _validator.TestValidateAsync(
			ValidRequest with
			{
				Password = Fixture.CreatePassword(config: PasswordConfig.All & ~PasswordConfig.Special)
			},
			cancellationToken: TestContext.Current.CancellationToken
		);
		result.ShouldHaveValidationErrorFor(x => x.Password);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Password.SpecialMissing);
	}

	[Fact]
	public async Task GivenTermsNotAccepted_ShouldHaveError()
	{
		var result = await _validator.TestValidateAsync(
			ValidRequest with { TermsAccepted = false },
			cancellationToken: TestContext.Current.CancellationToken
		);
		result.ShouldHaveValidationErrorFor(x => x.TermsAccepted);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Terms.NotAccepted);
	}

	[Fact]
	public async Task GivenEmptyClientId_ShouldHaveError()
	{
		var result = await _validator.TestValidateAsync(
			ValidRequest with { ClientId = string.Empty },
			cancellationToken: TestContext.Current.CancellationToken
		);
		result.ShouldHaveValidationErrorFor(x => x.ClientId);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Client.Missing);
	}

	[Fact]
	public async Task GivenNonExistingClientId_ShouldHaveError()
	{
		var result = await _validator.TestValidateAsync(
			ValidRequest with { ClientId = _invalidClientId },
			cancellationToken: TestContext.Current.CancellationToken
		);
		result.ShouldHaveValidationErrorFor(x => x.ClientId);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Client.NotFound);
	}

	[Fact]
	public async Task GivenEmptyRedirectUri_ShouldHaveError()
	{
		var result = await _validator.TestValidateAsync(
			ValidRequest with { RedirectUri = string.Empty },
			cancellationToken: TestContext.Current.CancellationToken
		);
		result.ShouldHaveValidationErrorFor(x => x.RedirectUri);
		result.Errors
			.Should()
			.Contain(error => error.ErrorMessage == LocalizationKey.Error.Redirect.Missing);
	}

	[Fact]
	public async Task GivenInvalidRedirectUri_ShouldHaveError()
	{
		var result = await _validator.TestValidateAsync(
			ValidRequest with { RedirectUri = Fixture.Create<string>() },
			cancellationToken: TestContext.Current.CancellationToken
		);
		result.ShouldHaveValidationErrorFor(x => x.RedirectUri);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Redirect.Invalid);
	}

	[Fact]
	public async Task GivenNonExistingRedirectUri_ShouldHaveError()
	{
		var result = await _validator.TestValidateAsync(
			ValidRequest with { RedirectUri = Fixture.CreateUri() },
			cancellationToken: TestContext.Current.CancellationToken
		);
		result.ShouldHaveValidationErrorFor(x => x.RedirectUri);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Redirect.NotFound);
	}
}
