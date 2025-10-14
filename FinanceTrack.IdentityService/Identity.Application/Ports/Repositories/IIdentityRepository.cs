using System;
using System.Threading.Tasks;
using Identity.Domain;

namespace Identity.Application.Ports.Repositories
{
    public interface IIdentityRepository
    {
        Task<User> GetUserByEmail(string email);
        Task<User?> UpdateUser(User user);
        Task<User> GetUserByUserId(Guid userId);
        Task<User?> CreateUser(User user, string password);
    }
}
