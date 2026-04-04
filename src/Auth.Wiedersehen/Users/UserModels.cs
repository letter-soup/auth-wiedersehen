using Auth.Wiedersehen.Configuration;
using Auth.Wiedersehen.Localization;
using FluentValidation;

namespace Auth.Wiedersehen.Users;

public record CreateUserRequest(string Email, string Password, bool TermsAccepted, string? ClientId = null, string? RedirectUri = null);

public record CreateUserResponse(string UserId, string? RedirectUri = null);

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
	public CreateUserRequestValidator(IConfiguration configuration, ILocalizer localizer)
	{
		var passwordMinLength = configuration.GetValue<int>(ConfigurationKey.Password.MinLength);

		RuleFor(x => x.Email)
			.NotEmpty()
			.WithMessage(localizer[LocalizationKey.Error.Email.Missing])
			.EmailAddress()
			.WithMessage(localizer[LocalizationKey.Error.Email.Invalid]);
		RuleFor(x => x.Password)
			.NotEmpty()
			.WithMessage(localizer[LocalizationKey.Error.Password.Missing])
			.MinimumLength(passwordMinLength)
			.WithMessage(localizer[LocalizationKey.Error.Password.TooShort, passwordMinLength])
			.Matches("[A-Z]")
			.WithMessage(localizer[LocalizationKey.Error.Password.UppercaseMissing])
			.Matches("[a-z]")
			.WithMessage(localizer[LocalizationKey.Error.Password.LowercaseMissing])
			.Matches("[0-9]")
			.WithMessage(localizer[LocalizationKey.Error.Password.DigitMissing])
			.Matches("[^a-zA-Z0-9]")
			.WithMessage(localizer[LocalizationKey.Error.Password.SpecialMissing]);
		RuleFor(x => x.TermsAccepted)
			.Equal(true)
			.WithMessage(localizer[LocalizationKey.Error.Terms.NotAccepted]);
	}
}
