using System.Security.Claims;
using Ardalis.SharedKernel;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.UserAggregate;
using FinanceTrack.Finance.Core.UserAggregate.Specifications;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Middlewares;

public class UserSyncMiddleware(IRepository<User> repository, IUnitOfWork unitOfWork) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.GetUserId();
            var email = context.User.FindFirstValue(ClaimTypes.Email);

            if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(email))
            {
                var user = await repository.FirstOrDefaultAsync(
                    new UserByIdSpec(Guid.Parse(userId))
                );

                if (user is null)
                {
                    await repository.AddAsync(User.Create(userId, email));
                }
                else if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    user.UpdateEmail(email);
                }

                await unitOfWork.SaveChangesAsync();
            }
        }

        await next(context);
    }
}
