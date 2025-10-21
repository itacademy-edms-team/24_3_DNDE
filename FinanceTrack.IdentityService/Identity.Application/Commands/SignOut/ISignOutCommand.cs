using Idenitity.Application.Commands.SignOut.Request;
using Idenitity.Application.Commands.SignOut.Response;
using Identity.Application.Commands;

namespace Idenitity.Application.Commands.SignOut
{
    public interface ISignOutCommand : ICommand<SignOutRequest, SignOutResponse> { }
}
