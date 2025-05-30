using System.Threading.Tasks;

namespace SpendingApp.Backend.Services
{
    public interface IEmailService
    {
        Task SendConfirmationEmail(string email, string token);
    }
}
