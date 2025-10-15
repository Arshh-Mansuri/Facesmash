using System.Text;
using FacesmashAPI.Data;
using FacesmashAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Load configuration
var configuration = builder.Configuration;

// 2️⃣ Add services
builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

// File upload deferred; blob storage service not registered

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new string[] { })
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// JWT authentication removed; users must login per request

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Facesmash API", Version = "v1" });
});

var app = builder.Build();

// 3️⃣ Middleware
app.UseCors("AllowReactApp");
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();
// Authentication removed
app.UseAuthorization();

app.MapControllers();

// 4️⃣ Seed DB if empty
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
