using Identity.Application.Commands.SignInUser.Request;
using Identity.Application.Commands.SignInUser.Response;

namespace Identity.Application.Commands.SignInUser
{
    public interface ISignInUserCommand : ICommand<SignInUserRequest, SignInUserResponse> { }
}
