using Auth.Wiedersehen.Exceptions;
using Auth.Wiedersehen.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Wiedersehen.Users;

[ApiController]
[Route("api/v1/user")]
public class UserController(
	IUserService userService,
	SignInManager<ApplicationUser> signInManager
) : Controller
{
	private readonly IUserService _userService = userService.Required(nameof(userService));
	private readonly SignInManager<ApplicationUser> _signInManager = signInManager.Required(nameof(signInManager));

	[HttpPost]
	[ProducesResponseType<CreateUserResponse>(StatusCodes.Status201Created)]
	[ProducesResponseType<ErrorDetails>(StatusCodes.Status400BadRequest)]
	[ProducesResponseType<ErrorDetails>(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> CreateUserAsync(
		[FromServices] IValidator<CreateUserRequest> validator,
		[FromServices] IRedirectUriValidator redirectUriValidator,
		[FromBody] CreateUserRequest request
	)
	{
		var validationResult = await validator.ValidateAsync(request);
		if (!validationResult.IsValid)
		{
			throw new HttpResponseException(validationResult.ToKeyValuePairs());
		}

		var validatedRedirectUri = await redirectUriValidator.ValidateAsync(request.ClientId, request.RedirectUri);

		var result = await _userService.CreateAsync(request);

		var user = await _signInManager.UserManager.FindByIdAsync(result.UserId);
		if (user is not null)
		{
			await _signInManager.SignInAsync(user, isPersistent: false);
		}

		return Created(string.Empty, result with { RedirectUri = validatedRedirectUri });
	}
}
