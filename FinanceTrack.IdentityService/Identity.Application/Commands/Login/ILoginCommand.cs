using Identity.Application.Commands.Login.Request;
using Identity.Application.Commands.Login.Response;

namespace Identity.Application.Commands.Login
{
    public interface ILoginCommand : ICommand<LoginRequest, LoginResponse> { }
}
