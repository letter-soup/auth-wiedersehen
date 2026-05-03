using Auth.Wiedersehen.Emails.Queries;
using Auth.Wiedersehen.Exceptions;
using Auth.Wiedersehen.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Wiedersehen.Emails;

[ApiController]
[Route("api/v1/email")]
public class EmailController(IIsEmailAvailableQuery isEmailAvailableQuery) : Controller
{
	private readonly IIsEmailAvailableQuery _isEmailAvailableQuery = isEmailAvailableQuery.Required(nameof(isEmailAvailableQuery));

	[HttpGet("is-available")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType<ErrorDetails>(StatusCodes.Status400BadRequest)]
	[ProducesResponseType<ErrorDetails>(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> EmailAvailableAsync(
		[FromServices] IValidator<EmailAvailableRequest> validator,
		[FromQuery] EmailAvailableRequest request
	)
	{
		var validationResult = await validator.ValidateAsync(request);
		if (!validationResult.IsValid)
		{
			throw new HttpResponseException(validationResult.ToKeyValuePairs());
		}

		var emailAvailable = await _isEmailAvailableQuery.ExecuteAsync(request.Email);

		return emailAvailable ? Ok() : Conflict();
	}
}
