using Identity.Application.UseCases.CreateUser.Request;
using Identity.Application.UseCases.CreateUser.Response;

namespace Identity.Application.UseCases.CreateUser
{
    public interface ICreateUserUseCase : IUseCase<CreateUserRequest, CreateUserResponse> { }
}
