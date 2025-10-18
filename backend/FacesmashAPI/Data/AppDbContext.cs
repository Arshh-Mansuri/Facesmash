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
            modelBuilder.Entity<User>().HasData(
    new User { Id = 1, Name = "Alice", Email = "alice.smith@student.uts.edu.au", PasswordHash = "123", Gender = "F", PhotoUrl = "https://randomuser.me/api/portraits/women/65.jpg", Rating = 1200 },
    new User { Id = 2, Name = "Bob", Email = "bob.jones@student.uts.edu.au", PasswordHash = "123", Gender = "M", PhotoUrl = "https://randomuser.me/api/portraits/men/52.jpg", Rating = 1200 },
    new User { Id = 3, Name = "Charlie", Email = "charlie.brown@student.uts.edu.au", PasswordHash = "123", Gender = "M", PhotoUrl = "https://randomuser.me/api/portraits/women/68.jpg", Rating = 1200 }
);

           
        }
    }
}
