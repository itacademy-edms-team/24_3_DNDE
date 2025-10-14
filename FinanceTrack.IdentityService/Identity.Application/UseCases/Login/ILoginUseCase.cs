using Identity.Application.UseCases.Login.Request;
using Identity.Application.UseCases.Login.Response;

namespace Identity.Application.UseCases.Login
{
    public interface ILoginUseCase : IUseCase<LoginRequest, LoginResponse> { }
}
