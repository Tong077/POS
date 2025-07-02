using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using POS_System.Models;

namespace POS_System.Services
{
    public class CustomUserManager : UserManager<ApplicationUser>
    {
        public CustomUserManager(
              IUserStore<ApplicationUser> store,
              IOptions<IdentityOptions> optionsAccessor,
              IPasswordHasher<ApplicationUser> passwordHasher,
              IEnumerable<IUserValidator<ApplicationUser>> userValidators,
              IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
              ILookupNormalizer keyNormalizer,
              IdentityErrorDescriber errors,
              IServiceProvider services,
              ILogger<UserManager<ApplicationUser>> logger)
              : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        // Override to skip username uniqueness validation
        public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            var errors = new List<IdentityError>();

          
            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                errors.Add(new IdentityError
                {
                    Code = "InvalidUserName",
                    Description = "Username cannot be empty."
                });
            }

          

          
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                errors.Add(new IdentityError
                {
                    Code = "InvalidEmail",
                    Description = "Email cannot be empty."
                });
            }

          

            return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }
    }
}
