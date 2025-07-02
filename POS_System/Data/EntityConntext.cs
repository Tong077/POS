using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;
using POS_System.Models;
using static Dapper.SqlMapper;

namespace POS_System.Data
{
    public class EntityConntext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public EntityConntext(DbContextOptions<EntityConntext> options) : base(options)
        { }
      
        public DbSet<Category> categories { get; set; }
        public DbSet<Customer> customers { get; set; }
        public DbSet<Inventory> inventories { get; set; }
        public DbSet<Product> products { get; set; }
        public DbSet<Supplier> suppliers { get; set; }
        public DbSet<SaleDetail> saleDetails { get; set; }
        public DbSet<Sale> sales { get; set; }
        public DbSet<Currency> currencies { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }

        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Permission> Permissions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ApplicationRole
            modelBuilder.Entity<ApplicationRole>(entity =>
            {
                entity.ToTable("ApplicationRoles");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.RoleName).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(false);
                entity.HasIndex(e => e.RoleName).IsUnique(false);
            });

            // Configure IdentityRole
            modelBuilder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable("AspNetRoles");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(128);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
                entity.Property(e => e.NormalizedName).IsRequired().HasMaxLength(256);
                entity.HasIndex(e => e.NormalizedName).IsUnique();
            });

            // Configure ApplicationUser
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("AspNetUsers");
                entity.Property(e => e.ImagePath).HasMaxLength(255);
                entity.Property(e => e.DisplayUsername).HasMaxLength(256);
                entity.HasIndex(e => e.NormalizedUserName).IsUnique(false);
                entity.HasIndex(e => e.NormalizedEmail).IsUnique(false);
            });

            
        }

    }
}
