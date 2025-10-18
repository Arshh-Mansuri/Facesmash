using FacesmashAPI.Data;
using FacesmashAPI.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Load configuration
var configuration = builder.Configuration;

// 2️⃣ Add services
builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

// Azure Blob Storage service
builder.Services.AddScoped<FacesmashAPI.Services.AzureBlobStorageService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new string[] { })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for sessions
    });
});

// Session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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
app.UseSession(); // Add session middleware
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
            // Female users
            new User { Name = "Alice", Email = "alice@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"), Gender = "F", PhotoUrl = "https://via.placeholder.com/300x300/ff69b4/ffffff?text=Alice", Rating = 1200 },
            new User { Name = "Emma", Email = "emma@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"), Gender = "F", PhotoUrl = "https://via.placeholder.com/300x300/ff69b4/ffffff?text=Emma", Rating = 1150 },
            new User { Name = "Sophia", Email = "sophia@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"), Gender = "F", PhotoUrl = "https://via.placeholder.com/300x300/ff69b4/ffffff?text=Sophia", Rating = 1300 },
            
            // Male users
            new User { Name = "Bob", Email = "bob@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"), Gender = "M", PhotoUrl = "https://via.placeholder.com/300x300/4169e1/ffffff?text=Bob", Rating = 1200 },
            new User { Name = "Charlie", Email = "charlie@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"), Gender = "M", PhotoUrl = "https://via.placeholder.com/300x300/4169e1/ffffff?text=Charlie", Rating = 1250 },
            new User { Name = "David", Email = "david@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"), Gender = "M", PhotoUrl = "https://via.placeholder.com/300x300/4169e1/ffffff?text=David", Rating = 1180 }
        );
        db.SaveChanges();
    }
}

app.Run();
