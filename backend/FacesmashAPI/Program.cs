using FacesmashAPI.Data;
using FacesmashAPI.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

// ============================================================================
// FACESMASH API - APPLICATION STARTUP CONFIGURATION
// ============================================================================
// This file configures the ASP.NET Core application, including:
// - Service registration and dependency injection
// - Middleware pipeline setup
// - Database configuration and seeding
// - CORS, session, and API documentation setup
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// CONFIGURATION SETUP
// ============================================================================
var configuration = builder.Configuration;

// ============================================================================
// SERVICE REGISTRATION
// ============================================================================

// Core API services
builder.Services.AddControllers();

// Database configuration - SQLite for development
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

// Azure Blob Storage service for photo uploads
builder.Services.AddScoped<FacesmashAPI.Services.AzureBlobStorageService>();

// Custom services demonstrating advanced programming concepts
builder.Services.AddScoped<FacesmashAPI.Interfaces.IAuthenticatable, FacesmashAPI.Services.AuthenticationService>();
builder.Services.AddScoped<FacesmashAPI.Interfaces.IProfileManager<FacesmashAPI.Models.User>, FacesmashAPI.Services.ProfileService<FacesmashAPI.Models.User>>();
builder.Services.AddScoped<FacesmashAPI.Services.LinqService>();
builder.Services.AddScoped<FacesmashAPI.Repositories.GenericRepository<FacesmashAPI.Models.User>>();

// CORS configuration for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new string[] { };
        
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for session-based authentication
    });
});

// Session services for user authentication
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session expires after 30 minutes of inactivity
    options.Cookie.HttpOnly = true;                 // Prevent XSS attacks
    options.Cookie.IsEssential = true;              // Required for GDPR compliance
});

// Swagger/OpenAPI documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(swaggerConfig =>
{
    swaggerConfig.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "Facesmash API", 
        Version = "v1",
        Description = "API for the Facesmash application - a social rating platform",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Facesmash Development Team"
        }
    });
});

// ============================================================================
// APPLICATION BUILD AND MIDDLEWARE PIPELINE
// ============================================================================
var app = builder.Build();

// Configure middleware pipeline in correct order
if (app.Environment.IsDevelopment())
{
    // Enable Swagger UI in development
    app.UseSwagger();
    app.UseSwaggerUI(swaggerUiConfig =>
    {
        swaggerUiConfig.SwaggerEndpoint("/swagger/v1/swagger.json", "Facesmash API v1");
        swaggerUiConfig.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

// Security and CORS middleware
app.UseCors("AllowReactApp");
app.UseHttpsRedirection();

// Static files and session middleware
app.UseStaticFiles();
app.UseSession(); // Must be before UseAuthorization

// Authorization middleware
app.UseAuthorization();

// Map API controllers
app.MapControllers();

// ============================================================================
// DATABASE INITIALIZATION AND SEEDING
// ============================================================================
using (var serviceScope = app.Services.CreateScope())
{
    var databaseContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    try
    {
        // Ensure database is created
        databaseContext.Database.EnsureCreated();
        
        // Wait a moment for database to be fully ready
        Thread.Sleep(100);
        
        // Seed database with initial users if empty
        if (!databaseContext.Users.Any())
        {
            Console.WriteLine("Seeding database with initial users...");
            
            var initialUsers = new List<User>
            {
                // Female users with placeholder profile photos
                new User 
                { 
                    Name = "Alice Johnson", 
                    Email = "alice@example.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"), 
                    Gender = "F", 
                    PhotoUrl = "https://via.placeholder.com/300x300/ff69b4/ffffff?text=Alice", 
                    Rating = 1200,
                    Bio = "Love traveling and photography! 📸",
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new User 
                { 
                    Name = "Emma Wilson", 
                    Email = "emma@example.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"), 
                    Gender = "F", 
                    PhotoUrl = "https://via.placeholder.com/300x300/ff69b4/ffffff?text=Emma", 
                    Rating = 1150,
                    Bio = "Fitness enthusiast and coffee lover ☕",
                    CreatedAt = DateTime.UtcNow.AddDays(-25)
                },
                new User 
                { 
                    Name = "Sophia Davis", 
                    Email = "sophia@example.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"), 
                    Gender = "F", 
                    PhotoUrl = "https://via.placeholder.com/300x300/ff69b4/ffffff?text=Sophia", 
                    Rating = 1300,
                    Bio = "Artist and nature lover 🌿",
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                },
                
                // Male users with placeholder profile photos
                new User 
                { 
                    Name = "Bob Smith", 
                    Email = "bob@example.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"), 
                    Gender = "M", 
                    PhotoUrl = "https://via.placeholder.com/300x300/4169e1/ffffff?text=Bob", 
                    Rating = 1200,
                    Bio = "Tech enthusiast and gamer 🎮",
                    CreatedAt = DateTime.UtcNow.AddDays(-28)
                },
                new User 
                { 
                    Name = "Charlie Brown", 
                    Email = "charlie@example.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"), 
                    Gender = "M", 
                    PhotoUrl = "https://via.placeholder.com/300x300/4169e1/ffffff?text=Charlie", 
                    Rating = 1250,
                    Bio = "Musician and foodie 🎵",
                    CreatedAt = DateTime.UtcNow.AddDays(-22)
                },
                new User 
                { 
                    Name = "David Miller", 
                    Email = "david@example.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"), 
                    Gender = "M", 
                    PhotoUrl = "https://via.placeholder.com/300x300/4169e1/ffffff?text=David", 
                    Rating = 1180,
                    Bio = "Sports fan and outdoor adventurer ⚽",
                    CreatedAt = DateTime.UtcNow.AddDays(-18)
                }
            };

            databaseContext.Users.AddRange(initialUsers);
            databaseContext.SaveChanges();
            
            Console.WriteLine($"Successfully seeded database with {initialUsers.Count} users.");
        }
        else
        {
            Console.WriteLine("Database already contains users. Skipping seeding.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during database initialization: {ex.Message}");
        // Don't crash the application if seeding fails
    }
}

// ============================================================================
// APPLICATION STARTUP
// ============================================================================
Console.WriteLine("Starting Facesmash API...");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"Database: SQLite");
Console.WriteLine($"CORS Origins: {string.Join(", ", configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new string[] { })}");

app.Run();
