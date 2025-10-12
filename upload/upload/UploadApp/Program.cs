using UploadApp.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ?? Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ?? Configure SQLite database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=facesmash.db"));

// ?? Build the app
var app = builder.Build();

// ?? Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(); // ? Serve files from wwwroot

app.UseRouting();
app.UseAuthorization();

app.MapControllers(); // ? Map your API controllers

app.Run();
