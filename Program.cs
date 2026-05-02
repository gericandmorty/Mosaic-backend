using backend.Data;
using backend.Infrastructure.Firebase;
using backend.Modules.Auth;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("MosaicDb"));

// Register Infrastructure
builder.Services.AddSingleton<FirebaseService>();

// Register Modules
builder.Services.AddAuthModule();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
