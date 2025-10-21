namespace Idenitity.Application.Commands.SignOut.Request
{
    public class SignOutRequest : Commands.Request
    {
        public Guid UserId { get; set; }
    }
}
