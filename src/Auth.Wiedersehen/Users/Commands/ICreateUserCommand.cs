namespace Auth.Wiedersehen.Users.Commands;

public interface ICreateUserCommand
{
	Task<CreateUserResponse> ExecuteAsync(CreateUserRequest request);
}
