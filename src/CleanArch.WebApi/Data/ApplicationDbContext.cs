using Microsoft.EntityFrameworkCore;

namespace CleanArch.WebApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure entity mappings and relationships here
        }
        
        // Define DbSets for your entities
        // public DbSet<YourEntity> YourEntities { get; set; }
    }
}