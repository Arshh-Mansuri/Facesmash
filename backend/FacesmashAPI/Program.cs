using FacesmashAPI.Data;
using FacesmashAPI.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

// Add CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add DbContext with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=facesmash.db"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Use CORS
app.UseCors("AllowReactApp");

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created and seed test users
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        var users = new List<User>
        {
            new User { Name = "Alice", Email = "alice.smith@student.uts.edu.au", PasswordHash = "123", Gender = "F", PhotoUrl = "https://i.imgur.com/1.png", Rating = 1200 },
            new User { Name = "Bob", Email = "bob.jones@student.uts.edu.au", PasswordHash = "123", Gender = "M", PhotoUrl = "https://i.imgur.com/2.png", Rating = 1200 },
            new User { Name = "Charlie", Email = "charlie.brown@student.uts.edu.au", PasswordHash = "123", Gender = "M", PhotoUrl = "https://i.imgur.com/3.png", Rating = 1200 }
        };

        db.Users.AddRange(users);
        db.SaveChanges();
    }
}

app.Run();
