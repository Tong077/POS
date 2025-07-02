using Microsoft.AspNetCore.Authorization;
using POS_System.Service;
using System.Collections.Concurrent;

namespace POS_System.Services
{
    public class PolicyService : IPolicyRepositovy
    {
        private readonly ConcurrentDictionary<string, AuthorizationPolicy> _policies = new();

        public void AddPolicy(string policyName, Action<AuthorizationPolicyBuilder> configurePolicy)
        {
            var policyBuilder = new AuthorizationPolicyBuilder();
            configurePolicy(policyBuilder);
            var policy = policyBuilder.Build();

            _policies[policyName] = policy;
        }

        public AuthorizationPolicy? GetPolicy(string policyName)
        {
            _policies.TryGetValue(policyName, out var policy);
            return policy;
        }

        public void RemovePolicy(string policyName)
        {
            _policies.TryRemove(policyName, out _);
        }
    }
}
