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
    public class ProtectedControllerTests
    {
        [Fact]
        public async Task Protected_Endpoint_Requires_Auth()
        {
            var dbName = $"TestDb_ProtectedNoAuth_{Guid.NewGuid()}";
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
            var response = await client.GetAsync("/api/protected/secret");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Protected_Endpoint_Returns_Data_With_Valid_Jwt_InMemoryDb()
        {
            var emailServiceMock = new Moq.Mock<IEmailService>();
            var dbName = $"TestDb_ProtectedEndpoint_{Guid.NewGuid()}";
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
            var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginDto);
            loginResponse.EnsureSuccessStatusCode();
            var json = await loginResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            var token = json.GetProperty("token").GetString();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync("/api/protected/secret");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }
}
