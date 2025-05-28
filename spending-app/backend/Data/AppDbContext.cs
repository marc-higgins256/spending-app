using Microsoft.EntityFrameworkCore;
using SpendingApp.Backend.Models;

namespace SpendingApp.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
    }
}
