using Auth.Wiedersehen.Exceptions;
using Auth.Wiedersehen.UnitTests.Extensions;
using Auth.Wiedersehen.Users;
using Auth.Wiedersehen.Users.Commands;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Auth.Wiedersehen.UnitTests;

public class UserControllerTests : UnitTestsBase
{
	private readonly Mock<ICreateUserCommand> _createUserCommandMock;
	private readonly Mock<IValidator<CreateUserRequest>> _validatorMock;
	private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
	private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
	private readonly UserController _controller;

	public UserControllerTests()
	{
		_createUserCommandMock = new Mock<ICreateUserCommand>();
		_validatorMock = new Mock<IValidator<CreateUserRequest>>();

		var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
		_userManagerMock = new Mock<UserManager<ApplicationUser>>(
			userStoreMock.Object,
			new Mock<IOptions<IdentityOptions>>().Object,
			new Mock<IPasswordHasher<ApplicationUser>>().Object,
			Array.Empty<IUserValidator<ApplicationUser>>(),
			Array.Empty<IPasswordValidator<ApplicationUser>>(),
			new Mock<ILookupNormalizer>().Object,
			new Mock<IdentityErrorDescriber>().Object,
			new Mock<IServiceProvider>().Object,
			new Mock<ILogger<UserManager<ApplicationUser>>>().Object
		);

		_signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
			_userManagerMock.Object,
			new Mock<IHttpContextAccessor>().Object,
			new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
			new Mock<IOptions<IdentityOptions>>().Object,
			new Mock<ILogger<SignInManager<ApplicationUser>>>().Object,
			new Mock<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>().Object,
			new Mock<IUserConfirmation<ApplicationUser>>().Object
		);

		_controller = new UserController(_createUserCommandMock.Object, _signInManagerMock.Object);
	}

	[Fact]
	public async Task GivenValidRequest_ShouldReturn201WithResponseAndCallSignIn()
	{
		var request = new CreateUserRequest(
			Fixture.CreateEmail(), Fixture.CreatePassword(), true, "client-1", "https://example.com/cb"
		);
		var userId = Fixture.Create<string>();
		var user = new ApplicationUser { Id = userId, Email = request.Email };

		_validatorMock.Setup(v => v.ValidateAsync(request, default))
			.ReturnsAsync(new ValidationResult());
		_createUserCommandMock.Setup(s => s.ExecuteAsync(request))
			.ReturnsAsync(new CreateUserResponse(userId));
		_userManagerMock.Setup(m => m.FindByIdAsync(userId))
			.ReturnsAsync(user);

		var result = await _controller.CreateUserAsync(_validatorMock.Object, request);

		var createdResult = result.Should().BeOfType<Microsoft.AspNetCore.Mvc.CreatedResult>().Subject;
		var response = createdResult.Value.Should().BeOfType<CreateUserResponse>().Subject;
		response.UserId.Should().Be(userId);
		response.RedirectUri.Should().Be(request.RedirectUri);

		_signInManagerMock.Verify(s => s.SignInAsync(user, false, null), Times.Once);
	}

	[Fact]
	public async Task GivenValidationFailure_ShouldThrowHttpResponseException()
	{
		var request = new CreateUserRequest(string.Empty, string.Empty, false);

		_validatorMock.Setup(v => v.ValidateAsync(request, default))
			.ReturnsAsync(new ValidationResult(
				[new ValidationFailure("Email", "error:email:missing")]
			));

		var act = () => _controller.CreateUserAsync(_validatorMock.Object, request);

		await act.Should().ThrowAsync<HttpResponseException>();
		_signInManagerMock.Verify(s => s.SignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<bool>(), It.IsAny<string?>()), Times.Never);
	}

	[Fact]
	public async Task GivenUserNotFoundAfterCreation_ShouldNotCallSignIn()
	{
		var request = new CreateUserRequest(Fixture.CreateEmail(), Fixture.CreatePassword(), true);
		var userId = Fixture.Create<string>();

		_validatorMock.Setup(v => v.ValidateAsync(request, default))
			.ReturnsAsync(new ValidationResult());
		_createUserCommandMock.Setup(s => s.ExecuteAsync(request))
			.ReturnsAsync(new CreateUserResponse(userId));
		_userManagerMock.Setup(m => m.FindByIdAsync(userId))
			.ReturnsAsync((ApplicationUser?)null);

		var result = await _controller.CreateUserAsync(_validatorMock.Object, request);

		result.Should().BeOfType<Microsoft.AspNetCore.Mvc.CreatedResult>();
		_signInManagerMock.Verify(s => s.SignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<bool>(), It.IsAny<string?>()), Times.Never);
	}
}
