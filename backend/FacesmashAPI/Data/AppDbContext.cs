using Microsoft.EntityFrameworkCore;
using FacesmashAPI.Models;

namespace FacesmashAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().HasData(
    new User { Id = 1, Name = "Alice", Email = "alice.smith@student.uts.edu.au", PasswordHash = "123", Gender = "F", PhotoUrl = "alice.jpg", Rating = 1200 },
    new User { Id = 2, Name = "Bob", Email = "bob.jones@student.uts.edu.au", PasswordHash = "123", Gender = "M", PhotoUrl = "bob.jpg", Rating = 1200 },
    new User { Id = 3, Name = "Charlie", Email = "charlie.brown@student.uts.edu.au", PasswordHash = "123", Gender = "M", PhotoUrl = "charlie.jpg", Rating = 1200 }
);

           
        }
    }
}
