using System.Net;
using System.Net.Http.Json;
using System.Net.Mail;

namespace Travel_Agency_Portal.Services;

public class AlertingService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AlertingService> _logger;

    public AlertingService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<AlertingService> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task NotifyAsync(string title, string message)
    {
        _logger.LogWarning("Alert triggered: {Title} - {Message}", title, message);

        var sent = false;
        var webhookUrl = _configuration["Alerting:SlackWebhookUrl"];
        if (!string.IsNullOrWhiteSpace(webhookUrl))
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                await client.PostAsJsonAsync(webhookUrl, new { text = $"{title}: {message}" });
                sent = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send Slack/webhook alert.");
            }
        }

        sent = await TrySendEmailAlertAsync(title, message) || sent;
        if (!sent)
        {
            _logger.LogInformation("No production alert channel is configured. Set Alerting:SlackWebhookUrl or Alerting:Email:*.");
        }
    }

    private async Task<bool> TrySendEmailAlertAsync(string title, string message)
    {
        var host = _configuration["Alerting:Email:SmtpHost"] ?? _configuration["Email:Smtp:Host"];
        var to = _configuration["Alerting:Email:To"];
        var recipients = SplitRecipients(to);
        if (string.IsNullOrWhiteSpace(host) || recipients.Count == 0)
        {
            return false;
        }

        try
        {
            var port = _configuration.GetValue("Alerting:Email:SmtpPort", _configuration.GetValue("Email:Smtp:Port", 587));
            var username = _configuration["Alerting:Email:Username"] ?? _configuration["Email:Smtp:Username"];
            var password = _configuration["Alerting:Email:Password"] ?? _configuration["Email:Smtp:Password"];
            var from = _configuration["Alerting:Email:From"] ?? _configuration["Email:Smtp:From"] ?? username ?? "alerts@travel-agency.local";
            var enableSsl = _configuration.GetValue("Alerting:Email:EnableSsl", _configuration.GetValue("Email:Smtp:EnableSsl", true));

            using var smtpClient = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl
            };

            if (!string.IsNullOrWhiteSpace(username))
            {
                smtpClient.Credentials = new NetworkCredential(username, password);
            }

            using var mail = new MailMessage
            {
                From = new MailAddress(from),
                Subject = $"Travel Agency alert: {title}",
                Body = message,
                IsBodyHtml = false
            };

            foreach (var recipient in recipients)
            {
                mail.To.Add(recipient);
            }

            await smtpClient.SendMailAsync(mail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email alert.");
            return false;
        }
    }

    private static List<string> SplitRecipients(string? recipients)
    {
        return (recipients ?? string.Empty)
            .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(recipient => !string.IsNullOrWhiteSpace(recipient))
            .ToList();
    }
}
