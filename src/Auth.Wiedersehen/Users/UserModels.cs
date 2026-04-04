using Auth.Wiedersehen.Configuration;
using Auth.Wiedersehen.Localization;
using Auth.Wiedersehen.Users.Queries;
using FluentValidation;

namespace Auth.Wiedersehen.Users;

public record CreateUserRequest(string Email, string Password, bool TermsAccepted, string? ClientId = null, string? RedirectUri = null);

public record CreateUserResponse(string UserId, string? RedirectUri = null);

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
	public CreateUserRequestValidator(
		IConfiguration configuration,
		ILocalizer localizer,
		IGetClientRedirectUrisQuery getClientRedirectUrisQuery
	)
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

		RuleFor(x => x.RedirectUri)
			.Must((request, _) => !(string.IsNullOrWhiteSpace(request.ClientId) ^ string.IsNullOrWhiteSpace(request.RedirectUri)))
			.WithMessage(localizer[LocalizationKey.Error.Redirect.InvalidRedirectUri]);

		RuleFor(x => x.ClientId)
			.MustAsync(async (request, clientId, _) =>
			{
				if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(request.RedirectUri))
					return true;

				var redirectUris = await getClientRedirectUrisQuery.ExecuteAsync(clientId);
				return redirectUris is not null;
			})
			.WithMessage(localizer[LocalizationKey.Error.Redirect.InvalidClient]);

		RuleFor(x => x.RedirectUri)
			.MustAsync(async (request, redirectUri, _) =>
			{
				if (string.IsNullOrWhiteSpace(request.ClientId) || string.IsNullOrWhiteSpace(redirectUri))
					return true;

				var redirectUris = await getClientRedirectUrisQuery.ExecuteAsync(request.ClientId);
				if (redirectUris is null)
					return true;

				return redirectUris.Any(r => r == redirectUri);
			})
			.WithMessage(localizer[LocalizationKey.Error.Redirect.InvalidRedirectUri]);
	}
}
