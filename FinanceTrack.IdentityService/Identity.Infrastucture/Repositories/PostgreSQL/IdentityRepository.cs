using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Identity.Application.Exceptions;
using Identity.Application.Exceptions;
using Identity.Application.Ports.Repositories;
using Identity.Application.UseCases.CreateUser.Request;
using Identity.Domain;
using Identity.Infrastucture.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Identity.Infrastucture.Repositories.PostgreSQL
{
    public class IdentityRepository : IIdentityRepository
    {
        private readonly AppIdentityContext _context;
        private readonly UserManager<User> _userManager;

        public IdentityRepository(
            AppIdentityContext appIdentityContext,
            UserManager<User> userManager
        )
        {
            _context = appIdentityContext;
            _userManager = userManager;
        }

        public async Task<User> GetUserByUserId(Guid userId)
        {
            return await _userManager.FindByIdAsync(userId.ToString());
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<User?> UpdateUser(User user)
        {
            var result = _userManager.UpdateAsync(user);
            if (!result.IsCompletedSuccessfully)
            {
                throw new UpdateUserException(
                    message: result.Exception?.Message,
                    innerException: result.Exception
                );
            }
            return await _userManager.FindByIdAsync(user.Id.ToString());
        }

        public async Task<User?> CreateUser(User user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new CreateUserException($"Failed to create user: {errorMessage}");
            }
            return await _userManager.FindByIdAsync(user.Id.ToString());
        }
    }
}
