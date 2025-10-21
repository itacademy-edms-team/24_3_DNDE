using Identity.Application.Commands.CreateUser.Request;
using Identity.Application.Commands.CreateUser.Response;

namespace Identity.Application.Commands.CreateUser
{
    public interface ICreateUserCommand : ICommand<CreateUserRequest, CreateUserResponse> { }
}
