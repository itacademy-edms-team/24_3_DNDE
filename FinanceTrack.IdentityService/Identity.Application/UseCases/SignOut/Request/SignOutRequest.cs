using System;

namespace Idenitity.Application.UseCases.SignOut.Request
{
    public class SignOutRequest : UseCases.Request
    {
        public Guid UserId { get; set; }
    }
}
