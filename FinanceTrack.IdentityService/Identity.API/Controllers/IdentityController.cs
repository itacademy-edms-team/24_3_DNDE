using System.Security.Cryptography.X509Certificates;
using Identity.Application.UseCases.CreateUser;
using Identity.Application.UseCases.CreateUser.Request;
using Identity.Application.UseCases.CreateUser.Response;
using Identity.Application.UseCases.Login;
using Identity.Application.UseCases.Login.Request;
using Identity.Application.UseCases.Login.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly ICreateUserUseCase _createUserUseCase;
        private readonly ILoginUseCase _loginUseCase;

        public IdentityController(ICreateUserUseCase createUserUseCase, ILoginUseCase loginUseCase)
        {
            _createUserUseCase = createUserUseCase;
            _loginUseCase = loginUseCase;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("CreateUser")]
        public async Task<CreateUserResponse> CreateUser(CreateUserRequest request)
        {
            return await _createUserUseCase.Execute(request);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("LoginUser")]
        public async Task<LoginResponse> LoginUser(LoginRequest request)
        {
            return await _loginUseCase.Execute(request);
        }
    }
}
