using System;
using System.Threading.Tasks;
using Identity.Domain;

namespace Identity.Application.Ports.Repositories
{
    public interface IIdentityRepository
    {
        Task<User> GetUserByEmailAsync(string email);
        Task UpdateUserAsync(User user);
        Task<User> GetUserByUserIdAsync(Guid userId);
        Task CreateUserAsync(User user, string password);
    }
}
