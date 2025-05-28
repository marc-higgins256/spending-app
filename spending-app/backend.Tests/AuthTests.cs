using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace backend.Tests
{
    public class AuthTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_Returns_Jwt_Token_On_Success()
        {
            var loginDto = new { username = "testuser", password = "Test1234!" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            json.TryGetProperty("token", out var tokenProp).Should().BeTrue();
            tokenProp.GetString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_Fails_With_Invalid_Credentials()
        {
            var loginDto = new { username = "testuser", password = "WrongPassword!" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_Fails_With_Missing_Fields()
        {
            var loginDto = new { username = "testuser" }; // missing password
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Protected_Endpoint_Requires_Auth()
        {
            var response = await _client.GetAsync("/api/protected/secret");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Protected_Endpoint_Returns_Data_With_Valid_Jwt()
        {
            // First, login to get a token
            var loginDto = new { username = "testuser", password = "Test1234!" };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            loginResponse.EnsureSuccessStatusCode();
            var json = await loginResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            var token = json.GetProperty("token").GetString();

            // Set JWT in Authorization header
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/protected/secret");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Register_Succeeds_With_New_User()
        {
            var registerDto = new { username = $"newuser_{System.Guid.NewGuid()}", password = "Test1234!", email = $"newuser_{System.Guid.NewGuid()}@test.com" };
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            json.TryGetProperty("username", out var usernameProp).Should().BeTrue();
            usernameProp.GetString().Should().Be(registerDto.username);
        }

        [Fact]
        public async Task Register_Fails_With_Duplicate_Username()
        {
            var unique = System.Guid.NewGuid();
            var registerDto = new { username = $"dupuser_{unique}", password = "Test1234!", email = $"dupuser_{unique}@test.com" };
            // First registration should succeed
            var response1 = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
            response1.EnsureSuccessStatusCode();
            // Second registration with same username should fail
            var registerDto2 = new { username = registerDto.username, password = "Test1234!", email = $"other_{unique}@test.com" };
            var response2 = await _client.PostAsJsonAsync("/api/auth/register", registerDto2);
            response2.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_Fails_With_Missing_Fields()
        {
            var registerDto = new { username = "", password = "", email = "" };
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }
}
