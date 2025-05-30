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
    public class EmailConfirmationTests
    {
        [Fact]
        public async Task ConfirmEmail_Fails_With_Invalid_Or_Expired_Token()
        {
            var dbName = $"TestDb_ConfirmEmailInvalid_{Guid.NewGuid()}";
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
            var response = await client.GetAsync("/api/auth/confirm-email?token=invalidtoken");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_Fails_If_Email_Not_Confirmed()
        {
            var dbName = $"TestDb_LoginNotConfirmed_{Guid.NewGuid()}";
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
            var registerDto = new { username = $"unconfirmed_{unique}", password = "Test1234!", email = $"unconfirmed_{unique}@test.com" };
            await client.PostAsJsonAsync("/api/auth/register", registerDto);
            var loginDto = new { username = registerDto.username, password = registerDto.password };
            var response = await client.PostAsJsonAsync("/api/auth/login", loginDto);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
            var msg = await response.Content.ReadAsStringAsync();
            msg.Should().Contain("Email not confirmed");
        }
    }
}
