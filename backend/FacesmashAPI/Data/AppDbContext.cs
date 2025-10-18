using Microsoft.EntityFrameworkCore;
using FacesmashAPI.Models;

namespace FacesmashAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Gender).IsRequired().HasMaxLength(1);
                entity.Property(e => e.PhotoUrl).HasMaxLength(500);
                entity.Property(e => e.Bio).HasMaxLength(500);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure Message entity
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.SentAt).IsRequired();
                
                // Configure foreign key relationships
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.FromUserId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.ToUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
