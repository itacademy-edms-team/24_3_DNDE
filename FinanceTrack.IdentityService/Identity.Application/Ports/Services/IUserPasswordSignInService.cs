using Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.Ports.Services
{
    public interface IUserPasswordSignInService
    {
        Task<SignInResult> CheckPassword(User user, string password, bool lockoutOnFail = false);
    }
}
