using Microsoft.AspNetCore.Authorization;

namespace POS_System.Service
{
    public interface IPolicyRepositovy
    {
        void AddPolicy(string policyName, Action<AuthorizationPolicyBuilder> configurePolicy);
        AuthorizationPolicy GetPolicy(string policyName);
        void RemovePolicy(string policyName); // New method
    }
}
