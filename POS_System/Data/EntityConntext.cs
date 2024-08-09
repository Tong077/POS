using Microsoft.EntityFrameworkCore;

namespace POS_System.Data
{
    public class EntityConntext : DbContext
    {
        public EntityConntext(DbContextOptions<EntityConntext> options) : base(options)
        { }
    }
}
