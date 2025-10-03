using FacesmashAPI.Data;
using FacesmashAPI.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Register services BEFORE building app
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=facesmash.db"));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build(); // <- app built here

// 2️⃣ Middleware comes AFTER building the app
app.UseCors("AllowReactApp");
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// 3️⃣ Seed database if needed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        db.Users.AddRange(
            new User { Name = "Alice", Email = "alice@example.com", PasswordHash = "123", Gender = "F", PhotoUrl = "alice.jpg", Rating = 1200 },
            new User { Name = "Bob", Email = "bob@example.com", PasswordHash = "123", Gender = "M", PhotoUrl = "bob.jpg", Rating = 1200 },
            new User { Name = "Charlie", Email = "charlie@example.com", PasswordHash = "123", Gender = "M", PhotoUrl = "charlie.jpg", Rating = 1200 }
        );
        db.SaveChanges();
    }
}

app.Run();
