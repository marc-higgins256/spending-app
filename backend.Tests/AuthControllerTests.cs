using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using SpendingApp.Backend.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;

namespace backend.Tests
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task Login_Returns_Jwt_Token_On_Success_WithInMemoryDb()
        {
            var emailServiceMock = new Moq.Mock<IEmailService>();
            var dbName = $"TestDb_LoginSuccess_{Guid.NewGuid()}";
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SpendingApp.Backend.Data.AppDbContext>));
                        if (descriptor != null) services.Remove(descriptor);
                        services.AddDbContext<SpendingApp.Backend.Data.AppDbContext>(options =>
                            options.UseInMemoryDatabase(dbName));
                        services.AddSingleton<IEmailService>(emailServiceMock.Object);
                    });
                });
            var client = factory.CreateClient();
            var unique = Guid.NewGuid();
            var registerDto = new { username = $"testuser_{unique}", password = "Test1234!", email = $"testuser_{unique}@test.com" };
            var regResp = await client.PostAsJsonAsync("/api/auth/register", registerDto);
            regResp.EnsureSuccessStatusCode();
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<SpendingApp.Backend.Data.AppDbContext>();
                var user = db.Users.First(u => u.Username == registerDto.username);
                user.IsEmailConfirmed = true;
                user.EmailConfirmationToken = null;
                user.EmailConfirmationTokenExpires = null;
                db.SaveChanges();
            }
            var loginDto = new { username = registerDto.username, password = registerDto.password };
            var response = await client.PostAsJsonAsync("/api/auth/login", loginDto);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            json.TryGetProperty("token", out var tokenProp).Should().BeTrue();
            tokenProp.GetString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_Fails_With_Invalid_Credentials()
        {
            var dbName = $"TestDb_LoginInvalid_{Guid.NewGuid()}";
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SpendingApp.Backend.Data.AppDbContext>));
                        if (descriptor != null) services.Remove(descriptor);
                        services.AddDbContext<SpendingApp.Backend.Data.AppDbContext>(options =>
                            options.UseInMemoryDatabase(dbName));
                    });
                });
            var client = factory.CreateClient();
            var loginDto = new { username = "testuser", password = "WrongPassword!" };
            var response = await client.PostAsJsonAsync("/api/auth/login", loginDto);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_Fails_With_Missing_Fields()
        {
            var dbName = $"TestDb_LoginMissing_{Guid.NewGuid()}";
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SpendingApp.Backend.Data.AppDbContext>));
                        if (descriptor != null) services.Remove(descriptor);
                        services.AddDbContext<SpendingApp.Backend.Data.AppDbContext>(options =>
                            options.UseInMemoryDatabase(dbName));
                    });
                });
            var client = factory.CreateClient();
            var loginDto = new { username = "testuser" }; // missing password
            var response = await client.PostAsJsonAsync("/api/auth/login", loginDto);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_Succeeds_With_New_User()
        {
            var emailServiceMock = new Moq.Mock<IEmailService>();
            var dbName = $"TestDb_RegisterNew_{Guid.NewGuid()}";
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SpendingApp.Backend.Data.AppDbContext>));
                        if (descriptor != null) services.Remove(descriptor);
                        services.AddDbContext<SpendingApp.Backend.Data.AppDbContext>(options =>
                            options.UseInMemoryDatabase(dbName));
                        services.AddSingleton<IEmailService>(emailServiceMock.Object);
                    });
                });
            var client = factory.CreateClient();
            var registerDto = new { username = $"newuser_{Guid.NewGuid()}", password = "Test1234!", email = $"newuser_{Guid.NewGuid()}@test.com" };
            var response = await client.PostAsJsonAsync("/api/auth/register", registerDto);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            json.TryGetProperty("username", out var usernameProp).Should().BeTrue();
            usernameProp.GetString().Should().Be(registerDto.username);
            // Optionally verify the email was attempted to be sent
            emailServiceMock.Verify(x => x.SendConfirmationEmail(registerDto.email, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Register_Fails_With_Duplicate_Username()
        {
            var dbName = $"TestDb_RegisterDuplicateUsername_{Guid.NewGuid()}";
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SpendingApp.Backend.Data.AppDbContext>));
                        if (descriptor != null) services.Remove(descriptor);
                        services.AddDbContext<SpendingApp.Backend.Data.AppDbContext>(options =>
                            options.UseInMemoryDatabase(dbName));
                    });
                });
            var client = factory.CreateClient();
            var unique = Guid.NewGuid();
            var registerDto1 = new { username = $"duplicateuser_{unique}", password = "Test1234!", email = $"duplicateuser_{unique}@test.com" };
            var registerDto2 = new { username = $"duplicateuser_{unique}", password = "Test5678!", email = $"anotheremail_{unique}@test.com" };
            var response1 = await client.PostAsJsonAsync("/api/auth/register", registerDto1);
            response1.EnsureSuccessStatusCode();
            var response2 = await client.PostAsJsonAsync("/api/auth/register", registerDto2);
            response2.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_Fails_With_Duplicate_Email()
        {
            var dbName = $"TestDb_RegisterDuplicateEmail_{Guid.NewGuid()}";
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SpendingApp.Backend.Data.AppDbContext>));
                        if (descriptor != null) services.Remove(descriptor);
                        services.AddDbContext<SpendingApp.Backend.Data.AppDbContext>(options =>
                            options.UseInMemoryDatabase(dbName));
                    });
                });
            var client = factory.CreateClient();
            var unique = Guid.NewGuid();
            var registerDto1 = new { username = $"user_{unique}", password = "Test1234!", email = $"user_{unique}@test.com" };
            var registerDto2 = new { username = $"anotheruser_{unique}", password = "Test5678!", email = $"user_{unique}@test.com" };
            var response1 = await client.PostAsJsonAsync("/api/auth/register", registerDto1);
            response1.EnsureSuccessStatusCode();
            var response2 = await client.PostAsJsonAsync("/api/auth/register", registerDto2);
            response2.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_Fails_With_Missing_Fields()
        {
            var dbName = $"TestDb_RegisterMissingFields_{Guid.NewGuid()}";
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SpendingApp.Backend.Data.AppDbContext>));
                        if (descriptor != null) services.Remove(descriptor);
                        services.AddDbContext<SpendingApp.Backend.Data.AppDbContext>(options =>
                            options.UseInMemoryDatabase(dbName));
                    });
                });
            var client = factory.CreateClient();
            var registerDto = new { username = "userwithoutpassword", email = "email@test.com" }; // missing password
            var response = await client.PostAsJsonAsync("/api/auth/register", registerDto);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_Calls_SendConfirmationEmail()
        {
            var emailServiceMock = new Moq.Mock<IEmailService>();
            var dbName = $"TestDb_RegisterEmail_{Guid.NewGuid()}";
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SpendingApp.Backend.Data.AppDbContext>));
                        if (descriptor != null) services.Remove(descriptor);
                        services.AddDbContext<SpendingApp.Backend.Data.AppDbContext>(options =>
                            options.UseInMemoryDatabase(dbName));
                        services.AddSingleton<IEmailService>(emailServiceMock.Object);
                    });
                });
            var client = factory.CreateClient();
            var unique = Guid.NewGuid();
            var registerDto = new { username = $"testuser_{unique}", password = "Test1234!", email = $"testuser_{unique}@test.com" };
            var response = await client.PostAsJsonAsync("/api/auth/register", registerDto);
            response.EnsureSuccessStatusCode();
            emailServiceMock.Verify(
                x => x.SendConfirmationEmail(registerDto.email, It.IsAny<string>()),
                Times.Once
            );
        }
    }
}
