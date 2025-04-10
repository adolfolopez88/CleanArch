using CleanArch.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.WebApi.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure entity mappings and relationships here
            
            // Customize the ASP.NET Identity model
            modelBuilder.Entity<ApplicationUser>(b =>
            {
                // Customize the ASP.NET Identity model and add properties
                b.Property(u => u.FirstName).HasMaxLength(100);
                b.Property(u => u.LastName).HasMaxLength(100);
                b.Property(u => u.IsActive).HasDefaultValue(true);
                b.Property(u => u.IsDeleted).HasDefaultValue(false);
                b.Property(u => u.CreatedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");
            });

            // Configure Identity tables
            modelBuilder.Entity<IdentityRole>(b =>
            {
                b.ToTable("Roles");
            });

            modelBuilder.Entity<IdentityUserRole<string>>(b =>
            {
                b.ToTable("UserRoles");
            });

            modelBuilder.Entity<IdentityUserClaim<string>>(b =>
            {
                b.ToTable("UserClaims");
            });

            modelBuilder.Entity<IdentityUserLogin<string>>(b =>
            {
                b.ToTable("UserLogins");
            });

            modelBuilder.Entity<IdentityRoleClaim<string>>(b =>
            {
                b.ToTable("RoleClaims");
            });

            modelBuilder.Entity<IdentityUserToken<string>>(b =>
            {
                b.ToTable("UserTokens");
            });

            modelBuilder.Entity<ApplicationUser>(b =>
            {
                b.ToTable("Users");
            });
            
            // Seed initial roles
            SeedRoles(modelBuilder);
        }
        
        private void SeedRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole 
                { 
                    Id = "1", 
                    Name = "Admin", 
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new IdentityRole 
                { 
                    Id = "2", 
                    Name = "User", 
                    NormalizedName = "USER",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                }
            );
            
            // Seed admin user
            var hasher = new PasswordHasher<ApplicationUser>();
            var adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@cleanarch.com",
                NormalizedEmail = "ADMIN@CLEANARCH.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                FirstName = "Admin",
                LastName = "User",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow
            };
            
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin123!");
            
            modelBuilder.Entity<ApplicationUser>().HasData(adminUser);
            
            // Assign admin role to admin user
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = "1",
                    UserId = adminUser.Id
                }
            );
        }
        
        // Define DbSets for your entities
        // public DbSet<YourEntity> YourEntities { get; set; }
    }
}