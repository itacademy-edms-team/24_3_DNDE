using System;
using System.Threading.Tasks;
using Identity.Domain;

namespace Identity.Application.Ports.Services
{
    public interface IAuthTokenService
    {
        Task<string> GenerateAccessToken(User user);
        Task<string> GenerateRefreshToken(User user);
    }
}
