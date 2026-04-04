using Auth.Wiedersehen.Configuration;
using Auth.Wiedersehen.Localization;
using Auth.Wiedersehen.UnitTests.Extensions;
using Auth.Wiedersehen.Users;
using Auth.Wiedersehen.Users.Queries;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Configuration;

namespace Auth.Wiedersehen.UnitTests;

public class CreateUserRequestValidatorTests : UnitTestsBase
{
	private readonly Mock<IGetClientRedirectUrisQuery> _queryMock;
	private readonly CreateUserRequestValidator _validator;

	public CreateUserRequestValidatorTests()
	{
		_queryMock = new Mock<IGetClientRedirectUrisQuery>();
		_validator = new CreateUserRequestValidator(Configuration, Localizer, _queryMock.Object);
	}

	[Fact]
	public async Task GivenEmptyEmail_ShouldHaveError()
	{
		var model = new CreateUserRequest(string.Empty, Fixture.CreatePassword(), true);
		var result = await _validator.TestValidateAsync(model, cancellationToken: TestContext.Current.CancellationToken);
		result.ShouldHaveValidationErrorFor(x => x.Email);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Email.Missing);
	}

	[Fact]
	public async Task GivenInvalidEmail_ShouldHaveError()
	{
		var model = new CreateUserRequest(Fixture.Create<string>(), Fixture.CreatePassword(), true);
		var result = await _validator.TestValidateAsync(model, cancellationToken: TestContext.Current.CancellationToken);
		result.ShouldHaveValidationErrorFor(x => x.Email);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Email.Invalid);
	}

	[Fact]
	public async Task GivenEmptyPassword_ShouldHaveError()
	{
		var model = new CreateUserRequest(Fixture.CreateEmail(), string.Empty, true);
		var result = await _validator.TestValidateAsync(model, cancellationToken: TestContext.Current.CancellationToken);
		result.ShouldHaveValidationErrorFor(x => x.Password);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Password.Missing);
	}

	[Fact]
	public async Task GivenShortPassword_ShouldHaveError()
	{
		var passwordLength = Configuration.GetValue<int>(ConfigurationKey.Password.MinLength) - 1;
		var model = new CreateUserRequest(
			Fixture.CreateEmail(),
			Fixture.CreatePassword(passwordLength),
			true
		);
		var result = await _validator.TestValidateAsync(model, cancellationToken: TestContext.Current.CancellationToken);
		result.ShouldHaveValidationErrorFor(x => x.Password);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Password.TooShort);
	}

	[Fact]
	public async Task GivenPasswordWithoutUppercase_ShouldHaveError()
	{
		var model = new CreateUserRequest(
			Fixture.CreateEmail(),
			Fixture.CreatePassword(config: PasswordConfig.All & ~PasswordConfig.UpperCase),
			true
		);
		var result = await _validator.TestValidateAsync(model, cancellationToken: TestContext.Current.CancellationToken);
		result.ShouldHaveValidationErrorFor(x => x.Password);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Password.UppercaseMissing);
	}

	[Fact]
	public async Task GivenPasswordWithoutLowercase_ShouldHaveError()
	{
		var model = new CreateUserRequest(
			Fixture.CreateEmail(),
			Fixture.CreatePassword(config: PasswordConfig.All & ~PasswordConfig.LowerCase),
			true
		);
		var result = await _validator.TestValidateAsync(model, cancellationToken: TestContext.Current.CancellationToken);
		result.ShouldHaveValidationErrorFor(x => x.Password);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Password.LowercaseMissing);
	}

	[Fact]
	public async Task GivenPasswordWithoutDigit_ShouldHaveError()
	{
		var model = new CreateUserRequest(
			Fixture.CreateEmail(),
			Fixture.CreatePassword(config: PasswordConfig.All & ~PasswordConfig.Digits),
			true
		);
		var result = await _validator.TestValidateAsync(model, cancellationToken: TestContext.Current.CancellationToken);
		result.ShouldHaveValidationErrorFor(x => x.Password);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Password.DigitMissing);
	}

	[Fact]
	public async Task GivenPasswordWithoutSpecialCharacter_ShouldHaveError()
	{
		var model = new CreateUserRequest(
			Fixture.CreateEmail(),
			Fixture.CreatePassword(config: PasswordConfig.All & ~PasswordConfig.Special),
			true
		);
		var result = await _validator.TestValidateAsync(model, cancellationToken: TestContext.Current.CancellationToken);
		result.ShouldHaveValidationErrorFor(x => x.Password);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Password.SpecialMissing);
	}

	[Fact]
	public async Task GivenTermsNotAccepted_ShouldHaveError()
	{
		var model = new CreateUserRequest(Fixture.CreateEmail(), Fixture.CreatePassword(), false);
		var result = await _validator.TestValidateAsync(model, cancellationToken: TestContext.Current.CancellationToken);
		result.ShouldHaveValidationErrorFor(x => x.TermsAccepted);
		result.Errors
			.Should()
			.ContainSingle(error => error.ErrorMessage == LocalizationKey.Error.Terms.NotAccepted);
	}

	[Fact]
	public async Task GivenValidRequest_ShouldHaveNoError()
	{
		var model = new CreateUserRequest(Fixture.CreateEmail(), Fixture.CreatePassword(), true);
		var result = await _validator.TestValidateAsync(model, cancellationToken: TestContext.Current.CancellationToken);
		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public async Task GivenBothClientIdAndRedirectUriNull_ShouldHaveNoError()
	{
		var model = new CreateUserRequest(Fixture.CreateEmail(), Fixture.CreatePassword(), true);
		var result = await _validator.TestValidateAsync(model, cancellationToken: TestContext.Current.CancellationToken);
		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public async Task GivenOnlyClientId_ShouldHaveErrorOnRedirectUri()
	{
		var model = new CreateUserRequest(Fixture.CreateEmail(), Fixture.CreatePassword(), true, "client-1");
		var result = await _validator.TestValidateAsync(model, cancellationToken: TestContext.Current.CancellationToken);
		result.ShouldHaveValidationErrorFor(x => x.RedirectUri)
			.WithErrorMessage(LocalizationKey.Error.Redirect.InvalidRedirectUri);
	}

	[Fact]
	public async Task GivenOnlyRedirectUri_ShouldHaveErrorOnRedirectUri()
	{
		var model = new CreateUserRequest(Fixture.CreateEmail(), Fixture.CreatePassword(), true, null, "https://example.com/cb");
		var result = await _validator.TestValidateAsync(model, cancellationToken: TestContext.Current.CancellationToken);
		result.ShouldHaveValidationErrorFor(x => x.RedirectUri)
			.WithErrorMessage(LocalizationKey.Error.Redirect.InvalidRedirectUri);
	}

	[Fact]
	public async Task GivenClientNotFound_ShouldHaveErrorOnClientId()
	{
		_queryMock.Setup(q => q.ExecuteAsync("nonexistent-client"))
			.ReturnsAsync((IReadOnlyList<string>?)null);

		var model = new CreateUserRequest(Fixture.CreateEmail(), Fixture.CreatePassword(), true, "nonexistent-client", "https://example.com/cb");
		var result = await _validator.TestValidateAsync(model, cancellationToken: TestContext.Current.CancellationToken);
		result.ShouldHaveValidationErrorFor(x => x.ClientId)
			.WithErrorMessage(LocalizationKey.Error.Redirect.InvalidClient);
	}

	[Fact]
	public async Task GivenUriNotAllowed_ShouldHaveErrorOnRedirectUri()
	{
		_queryMock.Setup(q => q.ExecuteAsync("test-client"))
			.ReturnsAsync(new List<string> { "https://allowed.com/cb" });

		var model = new CreateUserRequest(Fixture.CreateEmail(), Fixture.CreatePassword(), true, "test-client", "https://notallowed.com/cb");
		var result = await _validator.TestValidateAsync(model, cancellationToken: TestContext.Current.CancellationToken);
		result.ShouldHaveValidationErrorFor(x => x.RedirectUri)
			.WithErrorMessage(LocalizationKey.Error.Redirect.InvalidRedirectUri);
	}

	[Fact]
	public async Task GivenValidClientAndUri_ShouldHaveNoError()
	{
		var expectedUri = "https://allowed.com/cb";
		_queryMock.Setup(q => q.ExecuteAsync("test-client"))
			.ReturnsAsync(new List<string> { expectedUri });

		var model = new CreateUserRequest(Fixture.CreateEmail(), Fixture.CreatePassword(), true, "test-client", expectedUri);
		var result = await _validator.TestValidateAsync(model, cancellationToken: TestContext.Current.CancellationToken);
		result.ShouldNotHaveAnyValidationErrors();
	}
}
