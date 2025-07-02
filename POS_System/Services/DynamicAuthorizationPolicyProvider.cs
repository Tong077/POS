using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using POS_System.Service;
using POS_System.Services;

public class DynamicAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
    private readonly IPolicyRepositovy _policyService;

    public DynamicAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options, IPolicyRepositovy policyService)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        _policyService = policyService;
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();
    public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        var policy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();

        return Task.FromResult(policy);
    }

}
