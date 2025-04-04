using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProductProgamming04042025.Pages.Models;

namespace ProductProgamming04042025.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<UserProfile> UserProfiles { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Фикс для DateTime, постгрес не поддерживает DateTime2
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetColumnType("timestamp without time zone");
                    }
                }
            }

            builder.Entity<UserProfile>()
                .HasOne(u => u.User)
                .WithOne()
                .HasForeignKey<UserProfile>(up => up.UserId);
        }
    }
}
