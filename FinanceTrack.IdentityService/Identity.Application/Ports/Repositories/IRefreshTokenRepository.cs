using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Domain;

namespace Identity.Application.Ports.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetRefreshTokenAsync(User user);
        Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken);
        Task CreateRefreshTokenAsync(RefreshToken refreshToken);
        Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
        Task RevokeTokenAsync(RefreshToken refreshToken);
        Task DeleteRefreshTokenAsync(RefreshToken refreshToken);
    }
}
