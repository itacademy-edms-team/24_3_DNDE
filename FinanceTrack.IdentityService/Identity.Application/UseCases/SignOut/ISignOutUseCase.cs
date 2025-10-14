using Idenitity.Application.UseCases.SignOut.Request;
using Idenitity.Application.UseCases.SignOut.Response;

namespace Idenitity.Application.UseCases.SignOut
{
    public interface ISignOutUseCase : IUseCase<SignOutRequest, SignOutResponse> { }
}
