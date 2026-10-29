namespace Host.Services;

public interface ITokenRefreshService
{
    Task RefreshIfNeededAsync(HttpContext context);
}
