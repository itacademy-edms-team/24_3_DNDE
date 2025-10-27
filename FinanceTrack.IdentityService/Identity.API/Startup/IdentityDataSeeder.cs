using Idenitity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Startup
{
    // Used to create initial data in db, for example: roles.
    public static class IdentityDataSeeder
    {
        private static readonly string[] RequiredRoles = new[] { "User", "Admin" };

        public static async Task SeedAsync(IServiceProvider rootServices, ILogger logger)
        {
            using var scope = rootServices.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

            foreach (var roleName in RequiredRoles)
            {
                var isRoleExists = await roleManager.RoleExistsAsync(roleName);

                if (isRoleExists)
                {
                    logger.LogDebug("Role {Role} already exists", roleName);
                }

                var result = await roleManager.CreateAsync(new Role { Name = roleName });

                if (!result.Succeeded)
                {
                    logger.LogError(
                        "Failed to create role {Role}: {Errors}",
                        roleName,
                        string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"))
                    );
                }
                else
                {
                    logger.LogInformation("Created role {Role}", roleName);
                }
            }
        }
    }
}
