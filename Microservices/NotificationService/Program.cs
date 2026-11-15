using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<MfaDeliveryWorker>();
builder.Services.AddSingleton<EmailSender>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new
{
    service = "notification-service",
    status = "healthy",
    utc = DateTime.UtcNow
}));

app.Run();

public sealed class MfaDeliveryWorker : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly EmailSender _emailSender;
    private readonly ILogger<MfaDeliveryWorker> _logger;

    public MfaDeliveryWorker(IConfiguration configuration, EmailSender emailSender, ILogger<MfaDeliveryWorker> logger)
    {
        _configuration = configuration;
        _emailSender = emailSender;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var connectionString = _configuration["RabbitMq:ConnectionString"];
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogWarning("RabbitMQ is not configured for notification service.");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                continue;
            }

            try
            {
                var queueName = _configuration["RabbitMq:MfaQueue"] ?? "travel-agency.mfa.delivery";
                var factory = new ConnectionFactory { Uri = new Uri(connectionString), DispatchConsumersAsync = true };

                using var connection = factory.CreateConnection("travel-agency-notification-service");
                using var channel = connection.CreateModel();
                channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
                channel.QueueDeclare(queueName + ".failed", durable: true, exclusive: false, autoDelete: false);
                channel.ConfirmSelect();
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                var disconnected = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                channel.ModelShutdown += (_, _) => disconnected.TrySetResult();

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (_, args) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(args.Body.ToArray());
                        var message = JsonSerializer.Deserialize<MfaDeliveryMessage>(json);
                        if (message is null)
                        {
                            channel.BasicReject(args.DeliveryTag, requeue: false);
                            return;
                        }
                        if (message.ExpiresAtUtc <= DateTime.UtcNow)
                        {
                            channel.BasicReject(args.DeliveryTag, requeue: false);
                            return;
                        }

                        var sent = await _emailSender.SendMfaCodeAsync(message);
                        if (sent)
                        {
                            channel.BasicAck(args.DeliveryTag, multiple: false);
                        }
                        else
                        {
                            // Bound retries so broken SMTP cannot create a CPU-intensive
                            // redelivery loop. Failed messages remain available for diagnosis.
                            var attempts = args.BasicProperties.Headers?.TryGetValue("x-attempts", out var count) == true
                                ? Convert.ToInt32(count) : 0;
                            var retry = channel.CreateBasicProperties();
                            retry.Persistent = true;
                            retry.ContentType = "application/json";
                            retry.Headers = new Dictionary<string, object> { ["x-attempts"] = attempts + 1 };
                            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                            channel.BasicPublish("", attempts >= 2 ? queueName + ".failed" : queueName,
                                retry, args.Body);
                            channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));
                            channel.BasicAck(args.DeliveryTag, multiple: false);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process MFA delivery message.");
                        channel.BasicReject(args.DeliveryTag, requeue: false);
                    }
                };

                channel.BasicConsume(queueName, autoAck: false, consumer);
                _logger.LogInformation("Notification service is consuming {QueueName}.", queueName);

                await disconnected.Task.WaitAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Notification service RabbitMQ connection failed. Retrying soon.");
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }
}

public sealed class EmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendMfaCodeAsync(MfaDeliveryMessage message)
    {
        var host = _configuration["Email:Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogWarning("SMTP is not configured. MFA code for {Email} was not emailed.", message.Email);
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
            _logger.LogError(ex, "Failed to send MFA email to {Email}.", message.Email);
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

public sealed class MfaDeliveryMessage
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}
