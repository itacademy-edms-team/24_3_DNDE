using System;

namespace Identity.Application.UseCases.CreateUser.Response
{
    public class CreateUserSuccessResponse : CreateUserResponse
    {
        public Guid UserId { get; internal set; }
    }
}
