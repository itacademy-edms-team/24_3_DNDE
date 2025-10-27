using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Application.Commands.RefreshTokens.Request;
using Identity.Application.Commands.RefreshTokens.Response;

namespace Identity.Application.Commands.RefreshTokens
{
    public interface IRefreshTokensCommand
        : ICommand<RefreshTokensRequest, RefreshTokensResponse> { }
}
