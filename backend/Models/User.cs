using System.ComponentModel.DataAnnotations;

namespace SpendingApp.Backend.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        // Add more fields as needed (e.g., FullName, DateOfBirth)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsEmailConfirmed { get; set; } = false;
        public string? EmailConfirmationToken { get; set; }
        public DateTime? EmailConfirmationTokenExpires { get; set; }
    }
}
