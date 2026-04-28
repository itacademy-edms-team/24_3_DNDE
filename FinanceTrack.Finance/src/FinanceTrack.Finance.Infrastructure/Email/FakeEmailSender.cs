using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.Infrastructure.Email;

public class FakeEmailSender(ILogger<FakeEmailSender> logger) : IEmailSender
{
    public Task SendEmailAsync(string to, string subject, string body)
    {
        logger.LogInformation(
            "Not actually sending an email to {to} with subject {subject}",
            to,
            subject
        );
        return Task.CompletedTask;
    }
}
