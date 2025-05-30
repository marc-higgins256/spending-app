using System.Threading.Tasks;
using SpendingApp.Backend.Services;

namespace SpendingApp.Backend.Services
{
    public class MockEmailService : IEmailService
    {
        public Task SendConfirmationEmail(string email, string token)
        {
            // Do nothing (mock)
            return Task.CompletedTask;
        }
    }
}