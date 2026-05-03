using Auth.Wiedersehen.Configuration;
using Auth.Wiedersehen.Localization;
using Auth.Wiedersehen.Users.Queries;
using FluentValidation;

namespace Auth.Wiedersehen.Users;

public record CreateUserRequest(
	string Email,
	string Password,
	bool TermsAccepted,
	string ClientId,
	string RedirectUri
);

public record CreateUserResponse(string UserId, string RedirectUri);

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
	public CreateUserRequestValidator(
		IConfiguration configuration,
		ILocalizer localizer,
		IGetClientByIdQuery getClientByIdQuery,
		IGetClientRedirectUrisQuery getClientRedirectUrisQuery
	)
	{
		var passwordMinLength = configuration.GetValue<int>(ConfigurationKey.Password.MinLength);

		RuleFor(x => x.Email)
			.Cascade(CascadeMode.Stop)
			.NotEmpty()
			.WithMessage(localizer[LocalizationKey.Error.Email.Missing])
			.EmailAddress()
			.WithMessage(localizer[LocalizationKey.Error.Email.Invalid]);

		RuleFor(x => x.Password)
			.Cascade(CascadeMode.Continue)
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

		RuleFor(x => x.ClientId)
			.Cascade(CascadeMode.Stop)
			.NotEmpty()
			.WithMessage(localizer[LocalizationKey.Error.Client.Missing])
			.MustAsync(async (clientId, ct) => await getClientByIdQuery.ExecuteAsync(clientId, ct) is not null)
			.WithMessage(localizer[LocalizationKey.Error.Client.NotFound]);

		RuleFor(x => x.RedirectUri)
			.Cascade(CascadeMode.Stop)
			.NotEmpty()
			.WithMessage(localizer[LocalizationKey.Error.Redirect.Missing])
			.Must(uriStr => Uri.TryCreate(uriStr, UriKind.Absolute, out var uri) &&
				(uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
			)
			.WithMessage(localizer[LocalizationKey.Error.Redirect.Invalid])
			.MustAsync(async (request, redirectUri, ct) =>
				(await getClientRedirectUrisQuery.ExecuteAsync(request.ClientId, ct)).Any(x => x == redirectUri)
			)
			.WithMessage(localizer[LocalizationKey.Error.Redirect.NotFound]);
	}
}
