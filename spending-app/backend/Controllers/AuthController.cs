using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendingApp.Backend.Data;
using SpendingApp.Backend.Models;
using SpendingApp.Backend.DTOs;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;

namespace SpendingApp.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AuthController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDTO uDTO)
        {
            if (uDTO == null)
                return BadRequest("User data is required.");

            IActionResult? validationResult = await ValidateSignUpAsync(uDTO);
            if (validationResult != null)
                return validationResult;

            string passwordHash = HashPassword(uDTO.Password);
            var token = Guid.NewGuid().ToString();
            var user = new User {
                Username = uDTO.Username!,
                PasswordHash = passwordHash,
                Email = uDTO.Email!,
                IsEmailConfirmed = false,
                EmailConfirmationToken = token,
                EmailConfirmationTokenExpires = DateTime.UtcNow.AddHours(24)
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // TODO: Send confirmation email here (see below)
            // await SendConfirmationEmail(user.Email, token);

            return Ok(new { user.Id, user.Username, user.Email });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.EmailConfirmationToken == token && u.EmailConfirmationTokenExpires > DateTime.UtcNow);
            if (user == null)
                return BadRequest("Invalid or expired confirmation token.");

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationTokenExpires = null;
            await _db.SaveChangesAsync();
            return Ok("Email confirmed. You can now log in.");
        }

        // Example email sending method (implement with your SMTP or email provider)
        private async Task SendConfirmationEmail(string email, string token)
        {
            // var confirmationLink = $"https://yourfrontend.com/confirm?token={token}";
            // Use SmtpClient or a service like SendGrid/Mailgun here
            await Task.CompletedTask;
        }

        private async Task<IActionResult?> ValidateSignUpAsync(UserDTO uDTO)
        {
            if (string.IsNullOrWhiteSpace(uDTO.Username) || string.IsNullOrWhiteSpace(uDTO.Password))
                return BadRequest("Username and password are required.");

            if (string.IsNullOrWhiteSpace(uDTO.Email))
                return BadRequest("Email is required.");

            // Check if username already exists
            if (await _db.Users.AnyAsync(u => u.Username == uDTO.Username))
                return BadRequest("Username already exists.");

            // Check if email already exists
            if (await _db.Users.AnyAsync(u => u.Email == uDTO.Email))
                return BadRequest("Email already exists.");

            // ...other validation (e.g., email format)...
            return null;
        }

        private static string HashPassword(string password)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Username) || string.IsNullOrWhiteSpace(loginDto.Password))
                return BadRequest("Username/email and password are required.");

            // Try to find user by username or email
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username || u.Email == loginDto.Username);
            if (user == null)
                return Unauthorized("Invalid username/email or password.");

            var passwordHash = HashPassword(loginDto.Password);
            if (user.PasswordHash != passwordHash)
                return Unauthorized("Invalid username/email or password.");

            // JWT generation
            var jwtSettings = HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: null,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Success: return user info and JWT
            return Ok(new { user.Id, user.Username, user.Email, token = tokenString });
        }
    }
}
