namespace FinanceTrack.Finance.Infrastructure.Email;

public class MailserverConfiguration()
{
    public string FromEmail { get; set; } = "noreply@financetrack.local";
    public string Hostname { get; set; } = "localhost";
    public int Port { get; set; } = 25;
}
