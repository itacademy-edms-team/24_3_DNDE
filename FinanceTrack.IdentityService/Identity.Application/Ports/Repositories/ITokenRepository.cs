using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Ports.Repositories
{
    public interface ITokenRepository
    {
        Task GetRefreshToken() { }
        Task CreateRefreshToken() { }
        Task UpdateRefreshToken() { }
        Task DeleteRefreshToken() { }
    }
}
