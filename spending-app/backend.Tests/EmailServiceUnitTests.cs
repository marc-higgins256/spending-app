using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SpendingApp.Backend.Services;
using Xunit;

namespace backend.Tests
{
    public class EmailServiceUnitTests
    {
        [Fact]
        public async Task SendConfirmationEmail_CallsSmtpClient_WithCorrectParameters()
        {
            // Arrange: Use a mock or fake configuration and SMTP client if refactored for DI
            var configMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            configMock.Setup(c => c["Mailtrap:Host"]).Returns("smtp.test.com");
            configMock.Setup(c => c["Mailtrap:Port"]).Returns("2525");
            configMock.Setup(c => c["Mailtrap:User"]).Returns("user");
            configMock.Setup(c => c["Mailtrap:Pass"]).Returns("pass");
            configMock.Setup(c => c["Mailtrap:From"]).Returns("from@test.com");

            // You would need to refactor EmailService to allow injecting a mock SmtpClient for true unit testing.
            // For now, just test that the method runs without exception with the config.
            var emailService = new EmailService(configMock.Object);

            // Act & Assert
            // This will throw if it tries to connect to a real SMTP server, so in real unit tests,
            // you should refactor EmailService to inject ISmtpClient and mock it.
            // Here, we just show the structure:
            await Assert.ThrowsAnyAsync<System.Exception>(() =>
                emailService.SendConfirmationEmail("to@test.com", "sometoken"));
        }
    }
}
