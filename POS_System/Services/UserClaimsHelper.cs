using System.Security.Claims;

namespace POS_System.Services
{
    public static class UserClaimsHelper
    {
        public static bool HasPermission(this ClaimsPrincipal user, string permission)
        {
            return user.HasClaim("Permissions", permission);
        }

    }
}
