using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace POS_System.Models
{
    public class AspNetRoles
    {
        public IKey Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public string ConcurrencyStamp { get; set; }
    }
}
