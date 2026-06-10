using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Services;

public class MfaDeliveryService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MfaDeliveryService> _logger;

    public MfaDeliveryService(IConfiguration configuration, ILogger<MfaDeliveryService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task DeliverCodeAsync(Users user, string code, DateTime expiresAtUtc)
    {
        var message = new MfaDeliveryMessage
        {
            UserId = user.U_Id,
            Email = user.U_Email,
            Name = $"{user.U_Name} {user.U_Surname}".Trim(),
            Code = code,
            ExpiresAtUtc = expiresAtUtc
        };

        if (TryPublishToQueue(message))
        {
            return;
        }

        if (await TrySendEmailAsync(message))
        {
            return;
        }

        _logger.LogWarning("MFA delivery was not sent because RabbitMQ and SMTP are not configured.");
    }

    private bool TryPublishToQueue(MfaDeliveryMessage message)
    {
        var connectionString = _configuration["RabbitMq:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return false;
        }

        try
        {
            var queueName = _configuration["RabbitMq:MfaQueue"] ?? "travel-agency.mfa.delivery";
            var factory = new ConnectionFactory { Uri = new Uri(connectionString), DispatchConsumersAsync = true };

            using var connection = factory.CreateConnection("travel-agency-api");
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.Type = "mfa.delivery";

            channel.BasicPublish(exchange: string.Empty, routingKey: queueName, basicProperties: properties, body: body);
            _logger.LogInformation("MFA delivery message published to RabbitMQ for {Email}.", message.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RabbitMQ MFA delivery failed; falling back to SMTP if configured.");
            return false;
        }
    }

    private async Task<bool> TrySendEmailAsync(MfaDeliveryMessage message)
    {
        var host = _configuration["Email:Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            return false;
        }

        try
        {
            var port = _configuration.GetValue("Email:Smtp:Port", 587);
            var username = _configuration["Email:Smtp:Username"];
            var password = _configuration["Email:Smtp:Password"];
            var from = _configuration["Email:Smtp:From"] ?? username ?? "no-reply@travel-agency.local";
            var enableSsl = _configuration.GetValue("Email:Smtp:EnableSsl", true);
            var recipients = SplitRecipients(_configuration["Email:Smtp:OverrideRecipients"]);
            if (recipients.Count == 0)
            {
                recipients.Add(message.Email);
            }

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
                Subject = "Your Travel Agency MFA code",
                Body = $"Hello {message.Name},\n\nYour MFA code is {message.Code}.\nIt expires at {message.ExpiresAtUtc:u}.\n\nTravel Agency",
                IsBodyHtml = false
            };

            foreach (var recipient in recipients)
            {
                mail.To.Add(recipient);
            }

            await smtpClient.SendMailAsync(mail);
            _logger.LogInformation("MFA email sent for {Email} to {RecipientCount} recipient(s).", message.Email, recipients.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SMTP MFA delivery failed.");
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
