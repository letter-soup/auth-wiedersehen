using Auth.Wiedersehen.Localization;
using FluentValidation;

namespace Auth.Wiedersehen.Emails;

public record EmailAvailableRequest(string Email);

public class EmailAvailableRequestValidator : AbstractValidator<EmailAvailableRequest>
{
	public EmailAvailableRequestValidator(ILocalizer localizer)
	{
		RuleFor(x => x.Email)
			.Cascade(CascadeMode.Stop)
			.NotEmpty()
			.WithMessage(localizer[LocalizationKey.Error.Email.Missing])
			.EmailAddress()
			.WithMessage(localizer[LocalizationKey.Error.Email.Invalid]);
	}
}
