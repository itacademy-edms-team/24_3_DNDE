using System;

namespace Identity.Application.Commands.CreateUser.Response
{
    public class CreateUserSuccessResponse : CreateUserResponse
    {
        public Guid UserId { get; internal set; }
    }
}
