using System;
using System.Threading.Tasks;
using Identity.Domain;

namespace Identity.Application.Ports.Repositories
{
    public interface IIdentityRepository
    {
        Task<User> GetUserByEmail(string email);
        Task UpdateUser(User user);
        Task<User> GetUserByUserId(Guid userId);
        Task CreateUser(User user, string password);
    }
}
