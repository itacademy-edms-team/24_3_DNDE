using System.Security.Cryptography.X509Certificates;
using Identity.Application.Commands.CreateUser;
using Identity.Application.Commands.CreateUser.Request;
using Identity.Application.Commands.CreateUser.Response;
using Identity.Application.Commands.Login;
using Identity.Application.Commands.Login.Request;
using Identity.Application.Commands.Login.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly ICreateUserCommand _createUserUseCase;
        private readonly ILoginCommand _loginUseCase;

        public IdentityController(ICreateUserCommand createUserUseCase, ILoginCommand loginUseCase)
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
