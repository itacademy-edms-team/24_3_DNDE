using System.Security.Claims;

namespace FinanceTrack.Finance.Web.Extensions;

public static class ClaimsPrincipalExtensions
{
  public static string? GetUserId(this ClaimsPrincipal user)
  {
    return user.FindFirstValue(ClaimTypes.NameIdentifier)
      ?? user.FindFirstValue("sub")
      ?? user.FindFirstValue("preferred_username"); // если когда-то захочешь юзать логин
  }
}
