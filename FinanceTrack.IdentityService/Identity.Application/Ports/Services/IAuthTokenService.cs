using System;
using System.Threading.Tasks;
using Identity.Domain;

namespace Identity.Application.Ports.Services
{
    public interface IAuthTokenService
    {
        Task<string> GenerateAccessTokenAsync(User user);
        Task<string> GenerateRefreshTokenAsync(User user);
    }
}
