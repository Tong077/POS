using Microsoft.AspNetCore.Authorization;
using POS_System.Service;
using POS_System.Services;
using System.Security.Claims;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IApplyPermissionRepository _permissionRepository;

    public PermissionAuthorizationHandler(IApplyPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        var hasPermission = await _permissionRepository.HasPermissionAsync(userId, requirement.Permission);
        System.Diagnostics.Debug.WriteLine($"Checking permission {requirement.Permission} for user {userId}: {hasPermission}");
        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }

}
