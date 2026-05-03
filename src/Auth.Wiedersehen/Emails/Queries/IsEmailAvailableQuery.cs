using Auth.Wiedersehen.Extensions;
using Auth.Wiedersehen.Users;
using Microsoft.AspNetCore.Identity;

namespace Auth.Wiedersehen.Emails.Queries;

internal sealed class IsEmailAvailableQuery(UserManager<ApplicationUser> userManager) : IIsEmailAvailableQuery
{
	private readonly UserManager<ApplicationUser> _userManager = userManager.Required(nameof(userManager));

	public async Task<bool> ExecuteAsync(string email)
	{
		return await _userManager.FindByEmailAsync(email) is null;
	}
}
