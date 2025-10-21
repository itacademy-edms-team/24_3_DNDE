using Identity.Application.Commands.SignUpUser.Request;
using Identity.Application.Commands.SignUpUser.Response;

namespace Identity.Application.Commands.SignUpUser
{
    public interface ISignUpUserCommand : ICommand<SignUpUserRequest, SignUpUserResponse> { }
}
