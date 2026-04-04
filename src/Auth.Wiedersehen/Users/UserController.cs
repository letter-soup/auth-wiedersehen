using Auth.Wiedersehen.Exceptions;
using Auth.Wiedersehen.Extensions;
using Auth.Wiedersehen.Users.Commands;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Wiedersehen.Users;

[ApiController]
[Route("api/v1/user")]
public class UserController(
	ICreateUserCommand createUserCommand,
	SignInManager<ApplicationUser> signInManager
) : Controller
{
	private readonly ICreateUserCommand _createUserCommand = createUserCommand.Required(nameof(createUserCommand));
	private readonly SignInManager<ApplicationUser> _signInManager = signInManager.Required(nameof(signInManager));

	[HttpPost]
	[ProducesResponseType<CreateUserResponse>(StatusCodes.Status201Created)]
	[ProducesResponseType<ErrorDetails>(StatusCodes.Status400BadRequest)]
	[ProducesResponseType<ErrorDetails>(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> CreateUserAsync(
		[FromServices] IValidator<CreateUserRequest> validator,
		[FromBody] CreateUserRequest request
	)
	{
		var validationResult = await validator.ValidateAsync(request);
		if (!validationResult.IsValid)
		{
			throw new HttpResponseException(validationResult.ToKeyValuePairs());
		}

		var result = await _createUserCommand.ExecuteAsync(request);

		var user = await _signInManager.UserManager.FindByIdAsync(result.UserId);
		if (user is not null)
		{
			await _signInManager.SignInAsync(user, isPersistent: false);
		}

		return Created(string.Empty, result);
	}
}
