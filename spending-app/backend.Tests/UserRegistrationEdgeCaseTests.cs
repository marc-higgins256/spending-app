using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;

namespace backend.Tests
{
    public class UserRegistrationEdgeCaseTests
    {
        [Fact]
        public async Task Register_Fails_With_Duplicate_Username()
        {
            var dbName = $"TestDb_DuplicateUsername_{Guid.NewGuid()}";
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
            var registerDto = new { username = $"dupuser_{unique}", password = "Test1234!", email = $"dupuser_{unique}@test.com" };
            var response1 = await client.PostAsJsonAsync("/api/auth/register", registerDto);
            response1.EnsureSuccessStatusCode();
            var registerDto2 = new { username = registerDto.username, password = "Test1234!", email = $"other_{unique}@test.com" };
            var response2 = await client.PostAsJsonAsync("/api/auth/register", registerDto2);
            response2.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_Fails_With_Duplicate_Email()
        {
            var dbName = $"TestDb_DuplicateEmail_{Guid.NewGuid()}";
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
            var registerDto = new { username = $"dupemail_{unique}", password = "Test1234!", email = $"dupemail_{unique}@test.com" };
            var response1 = await client.PostAsJsonAsync("/api/auth/register", registerDto);
            response1.EnsureSuccessStatusCode();
            var registerDto2 = new { username = $"other_{unique}", password = "Test1234!", email = registerDto.email };
            var response2 = await client.PostAsJsonAsync("/api/auth/register", registerDto2);
            response2.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_Fails_With_Missing_Fields()
        {
            var dbName = $"TestDb_RegisterMissing_{Guid.NewGuid()}";
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
            var registerDto = new { username = "", password = "", email = "" };
            var response = await client.PostAsJsonAsync("/api/auth/register", registerDto);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }
}
