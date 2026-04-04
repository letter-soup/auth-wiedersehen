using Auth.Wiedersehen.Exceptions;
using Auth.Wiedersehen.Extensions;
using Microsoft.AspNetCore.Identity;

namespace Auth.Wiedersehen.Users.Commands;

internal sealed class CreateUserCommand(UserManager<ApplicationUser> userManager) : ICreateUserCommand
{
	private readonly UserManager<ApplicationUser> _userManager = userManager.Required(nameof(userManager));

	public async Task<CreateUserResponse> ExecuteAsync(CreateUserRequest request)
	{
		var user = new ApplicationUser
		{
			Email = request.Email,
			UserName = request.Email,
			TermsAcceptanceTime = DateTime.UtcNow,
		};

		var result = await _userManager.CreateAsync(user, request.Password);

		return result.Succeeded
			? new CreateUserResponse(user.Id, request.RedirectUri)
			: throw new HttpResponseException(result.ToKeyValuePairs(), StatusCodes.Status409Conflict);
	}
}
