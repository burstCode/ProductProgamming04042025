using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProductProgamming04042025.Pages.Models;
using System.Reflection.Emit;

namespace ProductProgamming04042025.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<ChatRecord> ChatRecords { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserProfile>()
                .HasOne(u => u.User)
                .WithOne()
                .HasForeignKey<UserProfile>(up => up.UserId);


            builder.Entity<ChatRecord>(entity =>
            {
                entity.Property(e => e.CreatedAt)
                      .HasColumnType("timestamp with time zone");

                entity.Property(e => e.AppliedDate)
                      .HasColumnType("timestamp with time zone");
            });
        }
    }
}
