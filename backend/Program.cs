using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SpendingApp.Backend.Data;
using System.Security.Cryptography;
using System.Text;
using Pomelo.EntityFrameworkCore.MySql;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Allow DB provider to be swapped via environment variable
var dbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER") ?? "mysql";
if (dbProvider.ToLower() == "inmemory")
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("TestDb"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            MySqlServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
        ));
}

builder.Services.AddControllers();

// Add JWT authentication
IConfigurationSection jwtSettings = builder.Configuration.GetSection("Jwt");
string keyString = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is not set in configuration.");

byte[] key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!); // null-forgiving operator to suppress warning
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Register the correct email service depending on environment
// Only use the real EmailService; Moq is used for tests
builder.Services.AddScoped<SpendingApp.Backend.Services.IEmailService, SpendingApp.Backend.Services.EmailService>();

// Add CORS policy for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors("DevCors");
}

// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
