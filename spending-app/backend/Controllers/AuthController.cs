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
using MailKit.Net.Smtp;
using MimeKit;

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

            // Send confirmation email here (see below)
            await SendConfirmationEmail(user.Email, token);

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
            var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var smtpHost = config["Mailtrap:Host"];
            var portString = config["Mailtrap:Port"];
            int smtpPort = 2525;
            if (!int.TryParse(portString, out smtpPort))
            {
                smtpPort = 2525; // fallback to default Mailtrap port - removes stupid warning
                // Maybe throw an exception or log a warning here. Should talk to Rab?
            }
            var smtpUser = config["Mailtrap:User"];
            var smtpPass = config["Mailtrap:Pass"];
            var from = config["Mailtrap:From"];

            var message = new MimeKit.MimeMessage();
            message.From.Add(new MimeKit.MailboxAddress("Spending App", from));
            message.To.Add(new MimeKit.MailboxAddress("", email));
            message.Subject = "Confirm your email";
            var confirmationLink = $"http://localhost:5173/confirm?token={token}";
            message.Body = new MimeKit.TextPart("plain")
            {
                Text = $"Click the link to confirm your email: {confirmationLink}"
            };

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
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
