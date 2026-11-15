using System.Net;
using System.Net.Mail;

namespace Travel_Agency_Portal.Services;

public interface IPasswordResetDelivery
{
    Task SendAsync(string email, string token, DateTime expiresAt);
}

public sealed class PasswordResetDelivery(IConfiguration configuration) : IPasswordResetDelivery
{
    public async Task SendAsync(string email, string token, DateTime expiresAt)
    {
        var host = configuration["Email:Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
            throw new InvalidOperationException("Configure SMTP to deliver password resets.");
        using var client = new SmtpClient(host, configuration.GetValue("Email:Smtp:Port", 587))
        {
            EnableSsl = configuration.GetValue("Email:Smtp:EnableSsl", true),
            Timeout = 10000
        };
        var username = configuration["Email:Smtp:Username"];
        if (!string.IsNullOrWhiteSpace(username))
            client.Credentials = new NetworkCredential(username, configuration["Email:Smtp:Password"]);
        // Recovery must go to the account owner; never use development recipient overrides.
        using var mail = new MailMessage(configuration["Email:Smtp:From"] ?? "no-reply@travel-agency.local", email)
        {
            Subject = "Reset your Travel Agency password",
            Body = $"Your password reset code is:\n\n{token}\n\nIt expires at {expiresAt:u}. If you did not request this, ignore this message."
        };
        await client.SendMailAsync(mail);
    }
}
