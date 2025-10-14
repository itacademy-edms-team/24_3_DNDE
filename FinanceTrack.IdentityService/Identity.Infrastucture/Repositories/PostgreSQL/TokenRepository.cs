using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Application.Ports.Repositories;

namespace Identity.Infrastucture.Repositories.PostgreSQL
{
    public class TokenRepository : ITokenRepository
    {
        public private readonly Task CreateRefreshToken(Guid uid, string rt) { }

        public Task DeleteRefreshToken() { }
    }
}
