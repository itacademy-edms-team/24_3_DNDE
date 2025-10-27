using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Application.Ports.Services;
using Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Identity.Infrastucture.Services.Auth
{
    public class UserPasswordSignInService(SignInManager<User> signInManager)
        : IUserPasswordSignInService
    {
        public async Task<SignInResult> CheckPassword(
            User user,
            string password,
            bool lockoutOnFail = false
        )
        {
            return await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFail);
        }
    }
}
